import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { checkAuth as apiCheckAuth, login as apiLogin, logout as apiLogout } from "./api";

interface AuthContextValue {
  isAuthed: boolean | null;
  login: (code: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthed, setIsAuthed] = useState<boolean | null>(null);

  useEffect(() => {
    apiCheckAuth().then(setIsAuthed);
  }, []);

  const login = useCallback(async (code: string) => {
    await apiLogin(code);
    setIsAuthed(true);
  }, []);

  const logout = useCallback(async () => {
    try {
      await apiLogout();
    } catch {
      // sign-out failures are non-critical
    }
    setIsAuthed(false);
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthed, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
