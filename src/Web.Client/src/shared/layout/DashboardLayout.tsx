import { Suspense, useCallback, useMemo } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Home, List, LogOut, Plus, Users } from 'lucide-react';
import { paths } from '../../app/routes/paths';
import { useAuth } from '../../features/auth/AuthContext';
import { getUser } from '../../features/auth/api';
import { useMyLeads } from '../../features/inquiries/queries';
import { countUnread } from '../../features/inquiries/readTracking';
import { Wordmark } from '../components/Logo';
import { Button } from '../ui/button';
import { Skeleton } from '../ui/skeleton';
import { cn } from '../ui/utils';

/**
 * Owner portal shell. Rendered by a layout route beneath `<ProtectedRoute>`, so
 * the sidebar and its two queries only ever mount for a signed-in owner, and
 * they survive navigation between the portal's pages.
 */
export function DashboardLayout() {
  const { userId, logout } = useAuth();
  const navigate = useNavigate();

  const { data: user } = useQuery({
    queryKey: ['user', userId],
    queryFn: () => getUser(userId as string),
    enabled: Boolean(userId),
    staleTime: 5 * 60_000,
  });

  const { data: leads } = useMyLeads({}, 1, 50);

  const unreadLeads = useMemo(
    () => (leads ? countUnread(leads.items.map((lead) => lead.id)) : 0),
    [leads],
  );

  const handleLogout = useCallback(async () => {
    await logout();
    navigate(paths.properties.root);
  }, [logout, navigate]);

  const links = useMemo(
    () => [
      { to: paths.manager.dashboard, label: 'Overview', Icon: Home, end: true, badge: 0 },
      { to: paths.manager.properties, label: 'Listings', Icon: List, end: true, badge: 0 },
      { to: paths.manager.leads, label: 'Leads', Icon: Users, end: false, badge: unreadLeads },
      { to: paths.manager.newProperty, label: 'New listing', Icon: Plus, end: false, badge: 0 },
    ],
    [unreadLeads],
  );

  return (
    <div className="min-h-screen bg-canvas lg:grid lg:grid-cols-[16rem_minmax(0,1fr)]">
      <aside className="flex flex-col border-b border-line bg-surface lg:sticky lg:top-0 lg:h-screen lg:border-b-0 lg:border-r">
        <div className="px-4 py-4">
          <Wordmark />
        </div>

        {/* Horizontal scroller on mobile, vertical rail from lg. */}
        <nav
          aria-label="Portal"
          className="flex gap-1 overflow-x-auto px-3 pb-3 lg:flex-col lg:overflow-visible lg:px-3 lg:pb-0"
        >
          {links.map(({ to, label, Icon, end, badge }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                cn(
                  'flex shrink-0 items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium outline-none',
                  'transition-colors duration-200 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta',
                  isActive ? 'bg-brand text-brand-fg' : 'text-ink-2 hover:bg-surface-3 hover:text-ink',
                )
              }
            >
              <Icon className="size-4" />
              {label}
              {badge > 0 && (
                <span className="tnum ml-auto grid size-5 shrink-0 place-items-center rounded-full bg-cta text-2xs font-bold text-cta-fg">
                  {badge}
                  <span className="sr-only"> unread leads</span>
                </span>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="mt-auto hidden items-center justify-between gap-2 border-t border-line px-4 py-3 lg:flex">
          {user && <span className="truncate text-sm text-ink-2">Hi, {user.firstName}</span>}
          <Button variant="ghost" size="sm" onClick={handleLogout}>
            <LogOut />
            Log out
          </Button>
        </div>
      </aside>

      <main className="min-w-0 px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
        <Suspense
          fallback={
            <div className="space-y-4" aria-hidden="true">
              <Skeleton className="h-8 w-48" />
              <Skeleton className="h-96 rounded-2xl" />
            </div>
          }
        >
          <Outlet />
        </Suspense>
      </main>
    </div>
  );
}
