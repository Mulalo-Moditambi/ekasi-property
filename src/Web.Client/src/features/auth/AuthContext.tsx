import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import {
  refreshAccessToken,
  setAccessToken,
  setSessionEndedHandler,
} from '../../shared/api/client';
import { login as loginRequest, logout as logoutRequest } from './api';
import { rolesFromClaims } from './roles';
import type { Role } from './roles';

interface Identity {
  userId: string | null;
  roles: Role[];
}

interface AuthState extends Identity {
  isAuthenticated: boolean;
  /**
   * True until the initial refresh settles. Routes must wait on this rather than treating
   * a not-yet-restored session as anonymous.
   */
  isLoading: boolean;
  /** True when the identity carries at least one of `required` (empty = any role). */
  hasRole: (required: readonly Role[]) => boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

const ANONYMOUS: Identity = { userId: null, roles: [] };

const AuthContext = createContext<AuthState | undefined>(undefined);

function identityFromToken(token: string | null): Identity {
  if (!token) {
    return ANONYMOUS;
  }

  const segment = token.split('.')[1];
  if (!segment) {
    return ANONYMOUS;
  }

  try {
    // JWT payloads are base64url-encoded — restore standard base64 before decoding.
    const base64 = segment.replace(/-/g, '+').replace(/_/g, '/');
    const claims = JSON.parse(atob(base64)) as Record<string, unknown>;
    const sub = typeof claims.sub === 'string' ? claims.sub : null;

    /*
     * An expired token must read as anonymous immediately. Without this the app renders a
     * signed-in shell whose every request fails, and the user only learns the session ended
     * by watching things break.
     */
    if (typeof claims.exp === 'number' && claims.exp * 1000 <= Date.now()) {
      return ANONYMOUS;
    }

    return sub ? { userId: sub, roles: rolesFromClaims(claims) } : ANONYMOUS;
  } catch {
    return ANONYMOUS;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [identity, setIdentity] = useState<Identity>(ANONYMOUS);
  const [isLoading, setIsLoading] = useState(true);

  /*
   * The access token is memory-only, so a reload starts with nothing. The httpOnly refresh
   * cookie is the only surviving evidence of a session: try it once on mount, and treat
   * failure as "not signed in".
   */
  useEffect(() => {
    let cancelled = false;

    refreshAccessToken()
      .then((token) => {
        if (!cancelled) {
          setIdentity(identityFromToken(token));
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  // Lets the API layer end the session when a refresh fails mid-flight.
  useEffect(() => {
    setSessionEndedHandler(() => setIdentity(ANONYMOUS));

    return () => setSessionEndedHandler(() => {});
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const tokens = await loginRequest(email, password);
    setAccessToken(tokens.accessToken);
    setIdentity(identityFromToken(tokens.accessToken));
  }, []);

  const logout = useCallback(async () => {
    try {
      await logoutRequest();
    } finally {
      // The local session ends even if the revoke call fails, otherwise a network blip
      // would leave the user apparently signed in.
      setAccessToken(null);
      setIdentity(ANONYMOUS);
    }
  }, []);

  const value = useMemo<AuthState>(() => {
    const { userId, roles } = identity;

    return {
      userId,
      roles,
      isAuthenticated: userId !== null,
      isLoading,
      hasRole: (required) => required.length === 0 || required.some((role) => roles.includes(role)),
      login,
      logout,
    };
  }, [identity, isLoading, login, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider');
  }

  return context;
}
