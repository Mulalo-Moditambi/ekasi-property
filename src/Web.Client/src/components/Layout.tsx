import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function Layout() {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/');
  }

  return (
    <>
      <header className="site-header">
        <Link to="/" className="brand">
          ekasi<span>property</span>
        </Link>
        <nav>
          <NavLink to="/">Browse</NavLink>
          {isAuthenticated ? (
            <>
              <NavLink to="/list-property" className="cta">
                List a Property
              </NavLink>
              <button type="button" className="link-button" onClick={handleLogout}>
                Log out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login">Log in</NavLink>
              <NavLink to="/register" className="cta">
                Register
              </NavLink>
            </>
          )}
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
      <footer className="site-footer">
        Ekasi Property — backrooms, cottages &amp; houses ekasi. Built for the township market.
      </footer>
    </>
  );
}
