package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"time"

	"github.com/google/uuid"
	"github.com/pocketbase/pocketbase"
	"github.com/pocketbase/pocketbase/apis"
	"github.com/pocketbase/pocketbase/core"
	"github.com/pocketbase/pocketbase/plugins/ghupdate"
	"github.com/pocketbase/pocketbase/plugins/jsvm"
	"github.com/pocketbase/pocketbase/plugins/migratecmd"
	"github.com/pocketbase/pocketbase/tools/hook"
)

// Gets the city and the country of the user through a Geo API
func getCityCountry(ip string) (string, error) {
	url := "https://ipgeolocation.abstractapi.com/v1?api_key=2685a4c976684ccba339a0bc8ddc5ad9&fields=country,city"

	// Make HTTP GET request
	resp, err := http.Get(url + "&ip=" + ip)
	if err != nil {
		fmt.Printf("Error making request: %v\n", err)
		return "", err
	}
	defer resp.Body.Close()

	// Check if status code is OK
	if resp.StatusCode != http.StatusOK {
		fmt.Printf("Error: got status code %d\n", resp.StatusCode)
		return "", err
	}

	// Read response body
	body, err := io.ReadAll(resp.Body)
	if err != nil {
		fmt.Printf("Error reading response: %v\n", err)
		return "", err
	}

	// Unmarshal JSON into a map
	data := make(map[string]string)
	err = json.Unmarshal(body, &data)
	if err != nil {
		fmt.Printf("Error unmarshaling JSON: %v\n", err)
		return "", err
	}

	return data["city"] + ", " + data["country"], nil
}

func main() {
	app := pocketbase.New()

	app.OnRecordCreateRequest("messages").BindFunc(func(e *core.RecordRequestEvent) error {
		// Check if the request is from an admin
		if e.RequestEvent.Auth != nil && e.RequestEvent.Auth.IsSuperuser() {
			// Set author_id to the "Admin" user ID and mark as admin message
			e.Record.Set("isAdmin", true)
		} else {
			data := struct {
				Token   string `json:"token"`
				ChatId  string `json:"chatId"`
				Content string `json:"content"`
			}{}

			if err := e.BindBody(&data); err != nil {
				return e.BadRequestError("Bad call to create message", err.Error())
			}

			record, err := app.FindFirstRecordByData("chatTokens", "token", data.Token)
			if err != nil {
				return e.BadRequestError("Invalid token", err.Error())
			}

			if record.GetString("chatId") != data.ChatId {
				return e.UnauthorizedError("Token does not allow for access to requested chat", "Please check token credibility")
			}

			// TODO: Handle logged in users separately
			// if e.RequestEvent.Auth.Verified()
			e.Record.Set("chatId", data.ChatId)
			e.Record.Set("content", data.Content)
			e.Record.Set("isAdmin", false)
		}
		return e.Next()
	})

	app.OnServe().BindFunc(func(tokenRoute *core.ServeEvent) error {
		tokenRoute.Router.POST("/api/get-chat-token", func(e *core.RequestEvent) error {
			chatTokens, err := app.FindCollectionByNameOrId("chatTokens")
			if err != nil {
				return e.NotFoundError("Cannot find chat_tokens collection", err.Error())
			}

			chats, err := app.FindCollectionByNameOrId("chats")
			if err != nil {
				return e.NotFoundError("Cannot find chats collection", err.Error())
			}

			chatRecord := core.NewRecord(chats)
			// NOTE: consider using abstract api to set the name of the chat to the region/country of the user
			name, err := getCityCountry(e.RemoteIP())
			if err != nil {
				name = "unknown ip address"
			}
			chatRecord.Set("name", "Anonymous chat with user from "+name)

			if err := app.Save(chatRecord); err != nil {
				return e.InternalServerError("Failed to save chat record", err.Error())
			}

			token := uuid.NewString()

			chatTokenRecord := core.NewRecord(chatTokens)
			chatTokenRecord.Set("token", token)
			chatTokenRecord.Set("chatId", chatRecord.Id)
			chatTokenRecord.Set("expiresAt", time.Now().Add(24*time.Hour).Format(time.RFC3339))
			if err := app.Save(chatTokenRecord); err != nil {
				return e.InternalServerError("Failed to save token record", err.Error())
			}

			e.SetCookie(&http.Cookie{
				Name:     "chatToken",
				Value:    token,
				HttpOnly: true,
				MaxAge:   86400, // 24 hours
			})

			return e.JSON(http.StatusOK, map[string]string{
				"token":  token,
				"chatId": chatRecord.Id,
			})
		})

		tokenRoute.Router.POST("/api/quote", func(e *core.RequestEvent) error {
			requestBody := struct {
				CargoType      string  `json:"cargoType"`
				Weight         float64 `json:"weight"`
				DeparturePort  string  `json:"departurePort"`
				DepartureTime  string  `json:"departureTime"`
				ArrivalPort    string  `json:"arrivalPort"`
				ArrivalTime    string  `json:"arrivalTime"`
				Dimensions     string  `json:"dimensions"`
				AdditionalInfo string  `json:"additionalInfo,omitempty"`
				Fname          string  `json:"fname"`
				Lname          string  `json:"lname"`
				Email          string  `json:"email"`
				PhoneNumber    string  `json:"phoneNumber"`
			}{}

			err := e.BindBody(&requestBody)
			if err != nil {
				return e.BadRequestError("Missing request fields", err.Error())
			}

			customer, err := app.FindFirstRecordByData("customers", "email", requestBody.Email)

			var id string

			switch err {
			case sql.ErrNoRows: // Create a new customer with a matching email
				customersTable, err := app.FindCollectionByNameOrId("customers")
				if err != nil {
					return e.InternalServerError("Cannot find customers table", err.Error())
				}

				customerRecord := core.NewRecord(customersTable)
				customerRecord.Set("email", requestBody.Email)
				customerRecord.Set("phoneNumber", requestBody.PhoneNumber)
				customerRecord.Set("firstName", requestBody.Fname)
				customerRecord.Set("lastName", requestBody.Lname)

				err = app.Save(customerRecord)
				if err != nil {
					return e.InternalServerError("Cannot save new customer record", err.Error())
				}
				id = customerRecord.Id
			case nil:
				id = customer.Id
			default:
				return e.InternalServerError("Cannot access customers table", err.Error())
			}

			quotesTable, err := app.FindCollectionByNameOrId("requestedQuotes")
			if err != nil {
				return e.InternalServerError("Cannot find quotes table", err.Error())
			}

			quoteRecord := core.NewRecord(quotesTable)
			quoteRecord.Set("cargoType", requestBody.CargoType)
			quoteRecord.Set("weight", requestBody.Weight)
			quoteRecord.Set("departurePort", requestBody.DeparturePort)
			quoteRecord.Set("departureTime", requestBody.DepartureTime)
			quoteRecord.Set("arrivalPort", requestBody.ArrivalPort)
			quoteRecord.Set("arrivalTime", requestBody.ArrivalTime)
			quoteRecord.Set("dimensions", requestBody.Dimensions)
			quoteRecord.Set("additionalInfo", requestBody.AdditionalInfo)

			quoteRecord.Set("customer", id)

			err = app.Save(quoteRecord)

			if err != nil {
				return e.InternalServerError("Failed to save quote", err.Error())
			}

			return e.JSON(http.StatusOK, map[string]any{
				"message": "Quote request created successfully!",
			})
		})

		return tokenRoute.Next()
	})

	// ---------------------------------------------------------------
	// Optional plugin flags:
	// ---------------------------------------------------------------

	var hooksDir string
	app.RootCmd.PersistentFlags().StringVar(
		&hooksDir,
		"hooksDir",
		"",
		"the directory with the JS app hooks",
	)

	var hooksWatch bool
	app.RootCmd.PersistentFlags().BoolVar(
		&hooksWatch,
		"hooksWatch",
		true,
		"auto restart the app on pb_hooks file change; it has no effect on Windows",
	)

	var hooksPool int
	app.RootCmd.PersistentFlags().IntVar(
		&hooksPool,
		"hooksPool",
		15,
		"the total prewarm goja.Runtime instances for the JS app hooks execution",
	)

	var migrationsDir string
	app.RootCmd.PersistentFlags().StringVar(
		&migrationsDir,
		"migrationsDir",
		"",
		"the directory with the user defined migrations",
	)

	var automigrate bool
	app.RootCmd.PersistentFlags().BoolVar(
		&automigrate,
		"automigrate",
		true,
		"enable/disable auto migrations",
	)

	var publicDir string
	app.RootCmd.PersistentFlags().StringVar(
		&publicDir,
		"publicDir",
		defaultPublicDir(),
		"the directory to serve static files",
	)

	var indexFallback bool
	app.RootCmd.PersistentFlags().BoolVar(
		&indexFallback,
		"indexFallback",
		true,
		"fallback the request to index.html on missing static path, e.g. when pretty urls are used with SPA",
	)

	app.RootCmd.ParseFlags(os.Args[1:])

	// ---------------------------------------------------------------
	// Plugins and hooks:
	// ---------------------------------------------------------------

	// load jsvm (pb_hooks and pb_migrations)
	jsvm.MustRegister(app, jsvm.Config{
		MigrationsDir: migrationsDir,
		HooksDir:      hooksDir,
		HooksWatch:    hooksWatch,
		HooksPoolSize: hooksPool,
	})

	// migrate command (with js templates)
	migratecmd.MustRegister(app, app.RootCmd, migratecmd.Config{
		TemplateLang: migratecmd.TemplateLangJS,
		Automigrate:  automigrate,
		Dir:          migrationsDir,
	})

	// GitHub selfupdate
	ghupdate.MustRegister(app, app.RootCmd, ghupdate.Config{})

	// static route to serves files from the provided public dir
	// (if publicDir exists and the route path is not already defined)
	app.OnServe().Bind(&hook.Handler[*core.ServeEvent]{
		Func: func(e *core.ServeEvent) error {
			if !e.Router.HasRoute(http.MethodGet, "/{path...}") {
				e.Router.GET("/{path...}", apis.Static(os.DirFS(publicDir), indexFallback))
			}

			return e.Next()
		},
		Priority: 999, // execute as latest as possible to allow users to provide their own route
	})

	if err := app.Start(); err != nil {
		log.Fatal(err)
	}
}

// the default pb_public dir location is relative to the executable
// Do not use go run to run the app, because it will not work
// as expected (the executable will be in a temporary directory).
func defaultPublicDir() string {
	path := filepath.Join(filepath.Dir(os.Args[0]), "pb_public")
	fmt.Println("Using public directory:", path)
	return path
}
