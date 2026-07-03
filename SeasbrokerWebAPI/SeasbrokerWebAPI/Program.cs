using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Seasbroker.Infrastructure.Persistence;

using Seasbroker.Modules.Chat;

using Seasbroker.Modules.Identity;

using Seasbroker.Modules.Quote;

using Seasbroker.Modules.Vessel;

using Seasbroker.Modules.Cargo;

using Seasbroker.Modules.Matching;

using Seasbroker.Modules.Approval;

using Seasbroker.Modules.Notifications;



var builder = WebApplication.CreateBuilder(args);



// Add services to the container.



builder.Services.AddPersistence(

    builder.Configuration.GetConnectionString("Default")!);



builder.Services.AddIdentityModule(builder.Configuration);

builder.Services.AddChatModule(builder.Configuration);

builder.Services.AddQuoteModule();

builder.Services.AddVesselModule();

builder.Services.AddCargoModule();

builder.Services.AddMatchingModule(builder.Configuration);

builder.Services.AddApprovalModule();

builder.Services.AddNotificationsModule();



builder.Services.AddControllers()

    .AddIdentityModuleControllers()

    .AddChatModuleControllers()

    .AddQuoteModuleControllers()

    .AddVesselModuleControllers()

    .AddCargoModuleControllers()

    .AddMatchingModuleControllers()

    .AddApprovalModuleControllers()

    .AddNotificationsModuleControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();



// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Seasbroker API");
    });
}



app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors();

app.UseChatModule();

app.UseVesselModule();

app.UseCargoModule();

app.UseMatchingModule();

app.UseApprovalModule();

app.UseNotificationsModule();

app.UseIdentityModule();

app.UseAuthorization();



app.MapControllers();

app.MapChatModule();

app.MapNotificationsModule();

app.Run();
