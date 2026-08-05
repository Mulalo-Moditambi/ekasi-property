import { request } from '../../shared/api/client';

/**
 * Only the access token comes back as JSON — the refresh token is set by the API as an
 * httpOnly cookie, so it is deliberately absent here and unreadable from script.
 */
export interface AccessTokens {
  accessToken: string;
}

export function register(
  email: string,
  firstName: string,
  lastName: string,
  password: string,
): Promise<string> {
  return request<string>('/users/register', {
    method: 'POST',
    body: JSON.stringify({ email, firstName, lastName, password }),
  });
}

export function login(email: string, password: string): Promise<AccessTokens> {
  return request<AccessTokens>('/users/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });
}

/** Revokes the refresh token server-side and clears the cookie. */
export function logout(): Promise<void> {
  return request<void>('/users/logout', { method: 'POST' });
}

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export function getUser(userId: string): Promise<UserProfile> {
  return request<UserProfile>(`/users/${userId}`);
}
