/**
 * Role vocabulary for route authorisation.
 *
 * The backend does not issue a role claim yet — `TokenProvider` puts only `sub`
 * and `email` in the JWT, and `Domain.Users.User` has no role column. So today
 * every authenticated user resolves to `['owner']`, which matches the product:
 * anyone who signs up can list a property and receive leads.
 *
 * The plumbing is here so that adding `new Claim(ClaimTypes.Role, …)` server-side
 * is the *only* change needed to make `<ProtectedRoute roles={…} />` bite.
 */
export type Role = 'owner' | 'admin';

export const ALL_ROLES: readonly Role[] = ['owner', 'admin'];

function isRole(value: unknown): value is Role {
  return typeof value === 'string' && (ALL_ROLES as readonly string[]).includes(value);
}

/**
 * Reads roles from the standard `role` claim, which .NET emits as a bare string
 * for one role and an array for several.
 */
export function rolesFromClaims(claims: Record<string, unknown> | null): Role[] {
  if (!claims) {
    return [];
  }

  const raw = claims.role ?? claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const candidates = Array.isArray(raw) ? raw : [raw];
  const roles = candidates.filter(isRole);

  // Until the API issues the claim, an authenticated user is an owner.
  return roles.length > 0 ? roles : ['owner'];
}
