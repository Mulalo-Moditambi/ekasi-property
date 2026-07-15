const BASE_URL = '/api';

const TOKEN_KEY = 'ekasi.accessToken';

export function getAccessToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setAccessToken(token: string | null): void {
  if (token === null) {
    localStorage.removeItem(TOKEN_KEY);
  } else {
    localStorage.setItem(TOKEN_KEY, token);
  }
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

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  if (!(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  const token = getAccessToken();
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  const response = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (!response.ok) {
    throw await toApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
