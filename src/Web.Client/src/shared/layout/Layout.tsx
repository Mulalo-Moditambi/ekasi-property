import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';

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
        <div className="footer-inner">
          <div className="footer-brand">
            <Link to="/" className="brand">
              ekasi<span>property</span>
            </Link>
            <p>Backrooms, cottages &amp; houses — straight from the owners, ekasi.</p>
          </div>
          <nav className="footer-col" aria-label="Browse">
            <h4>Browse</h4>
            <Link to="/">All listings</Link>
            <Link to="/list-property">List a property</Link>
          </nav>
          <nav className="footer-col" aria-label="Account">
            <h4>Account</h4>
            {isAuthenticated ? (
              <button type="button" className="logout-button" onClick={handleLogout} title="Sign out of your account">
                <span>↗</span> Log out
              </button>
            ) : (
              <>
                <Link to="/login">Log in</Link>
                <Link to="/register">Register</Link>
              </>
            )}
          </nav>
          <div className="footer-col">
            <h4>Get in touch</h4>
            <a href="mailto:hello@ekasiproperty.co.za">hello@ekasiproperty.co.za</a>
          </div>
        </div>
        <div className="footer-bottom">
          <p>© {new Date().getFullYear()} Ekasi Property. Built for the township market.</p>
        </div>
      </footer>
    </>
  );
}
