import { Link } from 'react-router-dom';
import type { LucideIcon } from 'lucide-react';
import { Compass, Lock, TriangleAlert } from 'lucide-react';
import { Button } from '../../shared/ui/button';
import { paths } from './paths';

interface StatusPageProps {
  code: string;
  title: string;
  message: string;
  icon: LucideIcon;
  action?: { label: string; to: string };
}

/**
 * One layout for every terminal route state (403/404/crash) so they can't drift
 * apart visually. Rendered inside whichever shell the route sits in, which keeps
 * the header and footer in place — a bare error screen loses the user's way out.
 */
function StatusPage({ code, title, message, icon: Icon, action }: StatusPageProps) {
  return (
    <div className="mx-auto flex max-w-md flex-col items-center px-6 py-24 text-center">
      <span className="grid size-12 place-items-center rounded-2xl bg-surface-3 text-ink-3">
        <Icon className="size-6" />
      </span>
      <p className="tnum mt-5 text-2xs font-semibold uppercase tracking-[0.18em] text-ink-3">
        Error {code}
      </p>
      <h1 className="mt-2 text-2xl font-bold tracking-tight text-ink">{title}</h1>
      <p className="mt-2 text-sm leading-relaxed text-ink-2">{message}</p>
      <Button variant="cta" className="mt-6" asChild>
        <Link to={action?.to ?? paths.properties.root}>{action?.label ?? 'Browse listings'}</Link>
      </Button>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <StatusPage
      code="404"
      title="We couldn't find that page"
      message="The link may be out of date, or the listing behind it has since been removed."
      icon={Compass}
    />
  );
}

export function ForbiddenPage() {
  return (
    <StatusPage
      code="403"
      title="You don't have access to this"
      message="Your account is signed in, but it isn't permitted to open this part of the portal."
      icon={Lock}
      action={{ label: 'Go to your dashboard', to: paths.manager.dashboard }}
    />
  );
}

/** Router `errorElement` — catches render/loader throws anywhere in the subtree. */
export function RouteErrorPage() {
  return (
    <StatusPage
      code="500"
      title="Something went wrong"
      message="An unexpected error interrupted this page. Reloading usually clears it."
      icon={TriangleAlert}
    />
  );
}
