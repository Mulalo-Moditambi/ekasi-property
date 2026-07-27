import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { getAccessToken, setAccessToken } from '../../shared/api/client';
import { login as loginRequest } from './api';

interface AuthState {
  userId: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

function userIdFromToken(token: string | null): string | null {
  if (!token) {
    return null;
  }

  const segment = token.split('.')[1];
  if (!segment) {
    return null;
  }

  try {
    // JWT payloads are base64url-encoded — restore standard base64 before decoding.
    const base64 = segment.replace(/-/g, '+').replace(/_/g, '/');
    const payload = JSON.parse(atob(base64)) as { sub?: string };
    return payload.sub ?? null;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [userId, setUserId] = useState<string | null>(() => userIdFromToken(getAccessToken()));

  const login = useCallback(async (email: string, password: string) => {
    const tokens = await loginRequest(email, password);
    setAccessToken(tokens.accessToken);
    setUserId(userIdFromToken(tokens.accessToken));
  }, []);

  const logout = useCallback(() => {
    setAccessToken(null);
    setUserId(null);
  }, []);

  const value = useMemo(
    () => ({ userId, isAuthenticated: userId !== null, login, logout }),
    [userId, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider');
  }

  return context;
}
