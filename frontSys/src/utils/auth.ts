import { createCookieSessionStorage } from "react-router";

type SessionData = {
  userId: string;
};

type SessionFlashData = {
  error: string;
};

const { getSession, commitSession, destroySession } =
  createCookieSessionStorage<SessionData, SessionFlashData>({
      cookie: {
        name: "__session",
        domain: "reactrouter.com",
        httpOnly: true,
        maxAge: 60,
        path: "/",
        sameSite: "lax",
        secrets: ["s3cret1"],
        secure: true,
      },
    }
  );

export { getSession, commitSession, destroySession };
