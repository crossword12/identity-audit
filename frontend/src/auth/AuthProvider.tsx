import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from "react";
import { getCurrentUser, login as requestLogin } from "../api/authApi";
import type { AuthSession, LoginRequest } from "../types/auth";
import {
  AUTH_SESSION_EXPIRED_EVENT,
  clearAuthSession,
  getAuthSession,
  setAuthSession,
} from "./authStorage";
import { AuthContext, type AuthContextValue } from "./authContext";

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSession | null>(() =>
    getAuthSession(),
  );

  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isCancelled = false;

    async function restoreSession() {
      const storedSession = getAuthSession();

      if (!storedSession) {
        if (!isCancelled) {
          setSession(null);
          setIsLoading(false);
        }

        return;
      }

      try {
        const currentUser = await getCurrentUser();

        if (isCancelled) {
          return;
        }

        const verifiedSession: AuthSession = {
          ...storedSession,
          user: currentUser,
        };

        setAuthSession(verifiedSession);
        setSession(verifiedSession);
      } catch {
        if (!isCancelled) {
          clearAuthSession();
          setSession(null);
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false);
        }
      }
    }

    void restoreSession();

    return () => {
      isCancelled = true;
    };
  }, []);

  useEffect(() => {
    function handleExpiredSession() {
      clearAuthSession();
      setSession(null);
      setIsLoading(false);
    }

    window.addEventListener(AUTH_SESSION_EXPIRED_EVENT, handleExpiredSession);

    return () => {
      window.removeEventListener(
        AUTH_SESSION_EXPIRED_EVENT,
        handleExpiredSession,
      );
    };
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await requestLogin(request);

    const newSession: AuthSession = {
      accessToken: response.accessToken,
      expiresAt: response.expiresAt,
      user: response.user,
    };

    setAuthSession(newSession);
    setSession(newSession);
  }, []);

  const logout = useCallback(() => {
    clearAuthSession();
    setSession(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      token: session?.accessToken ?? null,
      isAuthenticated: session !== null,
      isLoading,
      login,
      logout,
    }),
    [session, isLoading, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
