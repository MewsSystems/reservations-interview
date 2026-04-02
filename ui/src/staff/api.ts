import ky, { HTTPError } from "ky";

export async function login(accessCode: string): Promise<void> {
  await ky.post("/api/staff/login", {
    headers: { "X-Staff-Code": accessCode },
  });
}

export async function logout(): Promise<void> {
  await ky.post("/api/staff/logout");
}

export async function checkAuth(): Promise<boolean> {
  try {
    await ky.get("/api/staff/check");
    return true;
  } catch (err) {
    if (err instanceof HTTPError && err.response.status === 401) {
      try { await logout(); } catch { /* already unauthenticated */ }
      return false;
    }
    throw err;
  }
}
