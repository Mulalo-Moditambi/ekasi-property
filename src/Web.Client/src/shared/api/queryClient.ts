import { QueryClient } from '@tanstack/react-query';
import { ApiError } from './client';

/**
 * Retrying a 401/403/404 is pointless — the answer will not change, and on the
 * dashboard it just delays the error state by a couple of seconds. Everything
 * else (network blips, 5xx) gets the default two retries.
 */
function shouldRetry(failureCount: number, error: unknown): boolean {
  if (error instanceof ApiError && error.status >= 400 && error.status < 500) {
    return false;
  }

  return failureCount < 2;
}

export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: shouldRetry,
        // Listings change on a human timescale; a browse→detail→back trip inside
        // half a minute should read from cache rather than re-hitting the API.
        staleTime: 30_000,
        gcTime: 5 * 60_000,
        refetchOnWindowFocus: false,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

/** Narrows unknown query/mutation errors to something displayable. */
export function errorMessage(error: unknown, fallback = 'Something went wrong'): string {
  return error instanceof Error ? error.message : fallback;
}
