import { Suspense, useCallback } from 'react';
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { LayoutDashboard, LogOut, Moon, Plus, Sun } from 'lucide-react';
import { useAuth } from '../../features/auth/AuthContext';
import { paths } from '../../app/routes/paths';
import { Wordmark } from '../components/Logo';
import { Button } from '../ui/button';
import { Skeleton } from '../ui/skeleton';
import { cn } from '../ui/utils';
import { useTheme } from '../useTheme';

/**
 * Public shell. It sits *above* the router outlet, so the header, theme toggle
 * and footer are never unmounted by a navigation — only the outlet swaps.
 */
export function AppShell() {
  const { isAuthenticated, logout } = useAuth();
  const { theme, toggle } = useTheme();
  const navigate = useNavigate();

  const handleLogout = useCallback(async () => {
    await logout();
    navigate(paths.properties.root);
  }, [logout, navigate]);

  return (
    <div className="flex min-h-screen flex-col bg-canvas">
      <header className="sticky top-0 z-40 border-b border-line bg-surface/85 backdrop-blur-md">
        <div className="mx-auto flex h-14 max-w-[1400px] items-center gap-3 px-4 sm:gap-4 sm:px-6">
          <Wordmark hideNameOnNarrow />

          <nav className="ml-2 hidden items-center gap-1 sm:flex">
            <NavLink
              to={paths.properties.root}
              className={({ isActive }) =>
                cn(
                  'rounded-md px-2.5 py-1.5 text-sm font-medium transition-colors',
                  isActive ? 'bg-brand-soft text-brand' : 'text-ink-2 hover:text-ink',
                )
              }
            >
              Browse
            </NavLink>
          </nav>

          <div className="ml-auto flex items-center gap-1.5">
            <Button
              variant="ghost"
              size="icon-sm"
              onClick={toggle}
              aria-label={theme === 'dark' ? 'Switch to light theme' : 'Switch to dark theme'}
            >
              {theme === 'dark' ? <Sun /> : <Moon />}
            </Button>

            {isAuthenticated ? (
              <>
                {/* Icon-only below sm: signed-in headers otherwise overflow at 375px. */}
                <Button variant="ghost" size="sm" asChild className="px-2 sm:px-3.5">
                  <Link to={paths.manager.dashboard} aria-label="Dashboard">
                    <LayoutDashboard />
                    <span className="hidden sm:inline">Dashboard</span>
                  </Link>
                </Button>
                <Button variant="ghost" size="icon-sm" onClick={handleLogout} aria-label="Log out">
                  <LogOut />
                </Button>
              </>
            ) : (
              <Button variant="ghost" size="sm" asChild>
                <Link to={paths.auth.login}>Log in</Link>
              </Button>
            )}

            {/* Listing a property is the site's high-intent action — the emerald accent. */}
            <Button variant="cta" size="sm" asChild>
              <Link to={isAuthenticated ? paths.manager.newProperty : paths.auth.register}>
                <Plus />
                <span className="hidden sm:inline">List a property</span>
                <span className="sm:hidden">List</span>
              </Link>
            </Button>
          </div>
        </div>
      </header>

      <main className="flex-1">
        {/* Boundary for the lazily-loaded routes below this shell. */}
        <Suspense fallback={<RouteFallback />}>
          <Outlet />
        </Suspense>
      </main>

      <SiteFooter isAuthenticated={isAuthenticated} onLogout={handleLogout} />
    </div>
  );
}

/** Holds the page's shape while its chunk downloads, so the footer doesn't jump up. */
function RouteFallback() {
  return (
    <div className="mx-auto max-w-[1400px] space-y-4 px-4 py-10 sm:px-6" aria-hidden="true">
      <Skeleton className="h-8 w-56" />
      <Skeleton className="h-64 rounded-2xl" />
    </div>
  );
}

function FooterColumn({ heading, children }: { heading: string; children: ReactNode }) {
  return (
    <nav aria-label={heading} className="flex flex-col gap-2">
      <h4 className="text-2xs font-semibold uppercase tracking-wider text-ink-3">{heading}</h4>
      {children}
    </nav>
  );
}

const footerLink =
  'text-sm text-ink-2 transition-colors hover:text-ink w-fit rounded-sm outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand';

function SiteFooter({
  isAuthenticated,
  onLogout,
}: {
  isAuthenticated: boolean;
  onLogout: () => void;
}) {
  return (
    <footer className="mt-12 border-t border-line bg-surface">
      <div className="mx-auto grid max-w-[1400px] gap-8 px-4 py-10 sm:grid-cols-2 sm:px-6 lg:grid-cols-4">
        <div className="flex flex-col gap-3">
          <Wordmark />
          <p className="max-w-xs text-sm leading-relaxed text-ink-2">
            Backrooms, cottages &amp; houses — straight from the owners, ekasi.
          </p>
        </div>

        <FooterColumn heading="Browse">
          <Link to={paths.properties.root} className={footerLink}>
            All listings
          </Link>
          <Link
            to={isAuthenticated ? paths.manager.newProperty : paths.auth.login}
            className={footerLink}
          >
            List a property
          </Link>
        </FooterColumn>

        <FooterColumn heading="Account">
          {isAuthenticated ? (
            <>
              <Link to={paths.manager.dashboard} className={footerLink}>
                Dashboard
              </Link>
              <button type="button" onClick={onLogout} className={cn(footerLink, 'text-left')}>
                Log out
              </button>
            </>
          ) : (
            <>
              <Link to={paths.auth.login} className={footerLink}>
                Log in
              </Link>
              <Link to={paths.auth.register} className={footerLink}>
                Register
              </Link>
            </>
          )}
        </FooterColumn>

        <FooterColumn heading="Get in touch">
          <a href="mailto:hello@ekasiproperty.co.za" className={footerLink}>
            hello@ekasiproperty.co.za
          </a>
        </FooterColumn>
      </div>

      <div className="border-t border-line">
        <p className="mx-auto max-w-[1400px] px-4 py-4 text-2xs text-ink-3 sm:px-6">
          © {new Date().getFullYear()} Ekasi Property. Built for the township market.
        </p>
      </div>
    </footer>
  );
}
