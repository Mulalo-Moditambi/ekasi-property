import { request } from '../../shared/api/client';

export interface AccessTokens {
  accessToken: string;
  refreshToken: string;
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
