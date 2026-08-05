import { CSRF_HEADER, ensureCsrfToken, getCsrfToken, renewCsrfToken } from './csrf';

const BASE_URL = '/api';

/*
 * The access token lives in memory only. Persisting it to localStorage would leave a
 * long-lived credential readable by any injected script; a reload instead rebuilds the
 * session from the httpOnly refresh cookie via `refreshAccessToken` below.
 */
let accessToken: string | null = null;

export function getAccessToken(): string | null {
  return accessToken;
}

export function setAccessToken(token: string | null): void {
  accessToken = token;
}

/**
 * Called when refreshing fails and the session is over. `AuthProvider` registers a
 * handler so it can drop its identity and redirect, without this module importing React.
 */
type SessionEndedHandler = () => void;

let onSessionEnded: SessionEndedHandler = () => {};

export function setSessionEndedHandler(handler: SessionEndedHandler): void {
  onSessionEnded = handler;
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: { description?: string }[];
}

async function toApiError(response: Response): Promise<ApiError> {
  let message = `Request failed (${response.status})`;

  try {
    const problem = (await response.json()) as ProblemDetails;
    message =
      problem.errors?.map((e) => e.description).join(' ') ||
      problem.detail ||
      problem.title ||
      message;
  } catch {
    // Not a ProblemDetails body; keep the generic message.
  }

  return new ApiError(response.status, message);
}

/*
 * Refresh tokens rotate server-side, so two refreshes racing would invalidate each other
 * and log the user out. Every caller therefore awaits the same in-flight promise — a
 * dashboard firing several queries at once produces exactly one refresh call.
 */
let refreshInFlight: Promise<string | null> | null = null;

// Deliberately bare `fetch`, not `request` — routing this through the wrapper would
// recurse when the refresh itself comes back 401.
function postRefresh(csrf: string | null): Promise<Response> {
  const headers = new Headers();

  if (csrf) {
    headers.set(CSRF_HEADER, csrf);
  }

  return fetch(`${BASE_URL}/users/refresh-token`, {
    method: 'POST',
    headers,
    credentials: 'include',
  });
}

export function refreshAccessToken(): Promise<string | null> {
  refreshInFlight ??= ensureCsrfToken()
    .then(async (csrf) => {
      let response = await postRefresh(csrf);

      // A 403 means the token or its cookie went stale — the data-protection key rotated,
      // or the cookie was cleared. One renewal covers that without bothering the user.
      if (response.status === 403) {
        response = await postRefresh(await renewCsrfToken());
      }

      if (!response.ok) {
        return null;
      }

      const body = (await response.json()) as { accessToken?: string };
      return body.accessToken ?? null;
    })
    .catch(() => null)
    .then((token) => {
      accessToken = token;
      return token;
    })
    .finally(() => {
      refreshInFlight = null;
    });

  return refreshInFlight;
}

const SAFE_METHODS = new Set(['GET', 'HEAD', 'OPTIONS']);

function buildHeaders(options: RequestInit, token: string | null): Headers {
  const headers = new Headers(options.headers);

  if (!(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  /*
   * Attached to every mutating request when we happen to hold a token, though the server
   * only enforces it on the cookie-authenticated ones. Sending it costs nothing and means a
   * future cookie-authenticated endpoint is covered by default rather than by remembering.
   * Its absence is never fatal here — only the endpoints that validate will object.
   */
  const csrf = getCsrfToken();
  if (csrf && !SAFE_METHODS.has((options.method ?? 'GET').toUpperCase())) {
    headers.set(CSRF_HEADER, csrf);
  }

  return headers;
}

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  let response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: buildHeaders(options, getAccessToken()),
    credentials: 'include',
  });

  /*
   * An expired access token is the common case here, and it is recoverable: refresh once
   * and replay the request. Only the retry's result is surfaced, so callers never see the
   * transient 401. A second 401 means the session is genuinely over.
   */
  if (response.status === 401) {
    const token = await refreshAccessToken();

    if (token === null) {
      onSessionEnded();
      throw await toApiError(response);
    }

    response = await fetch(`${BASE_URL}${path}`, {
      ...options,
      headers: buildHeaders(options, token),
      credentials: 'include',
    });

    if (response.status === 401) {
      onSessionEnded();
    }
  }

  /*
   * A stale CSRF token — expired, or its cookie cleared. Renewing and replaying once keeps
   * a long-idle tab working instead of failing the user's first action after returning.
   */
  if (response.status === 403) {
    const csrf = await renewCsrfToken();

    if (csrf !== null) {
      response = await fetch(`${BASE_URL}${path}`, {
        ...options,
        headers: buildHeaders(options, getAccessToken()),
        credentials: 'include',
      });
    }
  }

  if (!response.ok) {
    throw await toApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
