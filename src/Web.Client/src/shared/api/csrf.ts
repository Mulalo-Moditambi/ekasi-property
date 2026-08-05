/**
 * CSRF token handling for the cookie-authenticated endpoints (refresh and logout).
 *
 * Everything else authenticates with a Bearer header, which a cross-site page cannot set,
 * so those endpoints are not forgeable and deliberately do not pay for this.
 *
 * The server keeps the secret half in an httpOnly cookie and hands us the request half here;
 * the two are signed together, so holding one without the other proves nothing.
 */

const BASE_URL = '/api';

export const CSRF_HEADER = 'X-CSRF-TOKEN';

let csrfToken: string | null = null;

// Shared like the refresh promise: several callers can discover a missing token at once,
// and fetching it more than once would just churn the cookie.
let tokenInFlight: Promise<string | null> | null = null;

function fetchToken(): Promise<string | null> {
  tokenInFlight ??= fetch(`${BASE_URL}/antiforgery/token`, { credentials: 'include' })
    .then(async (response) => {
      if (!response.ok) {
        return null;
      }

      const body = (await response.json()) as { token?: string };
      return body.token ?? null;
    })
    .catch(() => null)
    .then((token) => {
      csrfToken = token;
      return token;
    })
    .finally(() => {
      tokenInFlight = null;
    });

  return tokenInFlight;
}

/** Returns a cached token, fetching one on first use. */
export function ensureCsrfToken(): Promise<string | null> {
  return csrfToken !== null ? Promise.resolve(csrfToken) : fetchToken();
}

/**
 * Discards the cached token and fetches a fresh one. Used after a 403, which is what the
 * server returns when the token expired or its cookie was cleared.
 */
export function renewCsrfToken(): Promise<string | null> {
  csrfToken = null;

  return fetchToken();
}

export function getCsrfToken(): string | null {
  return csrfToken;
}
