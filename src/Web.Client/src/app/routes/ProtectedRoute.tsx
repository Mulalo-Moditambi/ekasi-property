import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';
import type { Role } from '../../features/auth/roles';
import { loginWithRedirect, paths } from './paths';

interface ProtectedRouteProps {
  /** Empty (the default) means "any authenticated user". */
  roles?: readonly Role[];
}

/**
 * Layout-route guard: `<Route element={<ProtectedRoute roles={['owner']} />}>`.
 *
 * The two failure modes are deliberately different. Not signed in is a
 * *recoverable* state, so we bounce to login carrying the attempted URL and
 * `replace` the entry — otherwise Back from the login screen lands on the very
 * page that redirected, and the user ping-pongs. Signed in but not permitted is
 * *terminal*: redirecting would be a lie, so it renders a 403 in place.
 */
export function ProtectedRoute({ roles = [] }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, hasRole } = useAuth();
  const location = useLocation();

  /*
   * On a reload the session is restored asynchronously from the refresh cookie. Treating
   * that window as "not signed in" would bounce every reload of a guarded page to login
   * and lose the user's place, so hold the render until it settles.
   */
  if (isLoading) {
    return null;
  }

  if (!isAuthenticated) {
    const attempted = `${location.pathname}${location.search}`;
    return <Navigate to={loginWithRedirect(attempted)} replace />;
  }

  if (!hasRole(roles)) {
    return <Navigate to={paths.forbidden} replace />;
  }

  return <Outlet />;
}
