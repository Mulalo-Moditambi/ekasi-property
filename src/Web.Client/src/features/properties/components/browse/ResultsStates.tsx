import { Link } from 'react-router-dom';
import { SearchX, TriangleAlert } from 'lucide-react';
import { paths } from '../../../../app/routes/paths';
import { useAuth } from '../../../auth/AuthContext';
import { Button } from '../../../../shared/ui/button';
import { Skeleton } from '../../../../shared/ui/skeleton';
import { cn } from '../../../../shared/ui/utils';
import type { BrowseView } from '../../searchParams';

export function ResultsSkeleton({ view }: { view: BrowseView }) {
  return (
    <div
      className={cn(view === 'grid' ? 'grid gap-5 sm:grid-cols-2 xl:grid-cols-3' : 'flex flex-col gap-3')}
      aria-hidden="true"
    >
      {Array.from({ length: 6 }, (_, index) =>
        view === 'grid' ? (
          <div key={index} className="overflow-hidden rounded-2xl border border-line bg-surface shadow-sm">
            <Skeleton className="aspect-[4/3] w-full rounded-none" />
            <div className="space-y-2.5 p-4">
              <Skeleton className="h-7 w-32" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-2/3" />
              <Skeleton className="h-4 w-1/2 pt-1" />
            </div>
          </div>
        ) : (
          <div
            key={index}
            className="flex min-h-[132px] gap-4 overflow-hidden rounded-2xl border border-line bg-surface shadow-sm sm:min-h-[264px] sm:gap-6"
          >
            <Skeleton className="w-[132px] shrink-0 rounded-none sm:w-[272px] lg:w-[336px]" />
            <div className="flex flex-1 flex-col justify-between py-4 pr-4 sm:py-5 sm:pr-5">
              <div className="space-y-2.5">
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-5 w-3/5 sm:h-7" />
                <Skeleton className="h-4 w-2/5" />
              </div>
              <div className="mt-2 space-y-2.5">
                <Skeleton className="h-4 w-1/3" />
                <Skeleton className="h-4 w-1/2" />
              </div>
            </div>
          </div>
        ),
      )}
    </div>
  );
}

export function EmptyState({ hasFilters, onClear }: { hasFilters: boolean; onClear: () => void }) {
  return (
    <div className="flex flex-col items-center rounded-2xl border border-dashed border-line-strong bg-surface px-6 py-16 text-center">
      <SearchX className="size-7 text-ink-3" />
      <h2 className="mt-3 text-base font-semibold text-ink">No homes match</h2>
      <p className="mt-1 max-w-sm text-sm text-ink-2">
        {hasFilters
          ? 'Nothing here fits every filter. Try widening the price range or dropping a must-have.'
          : 'No places are listed right now — check back soon, new ones are added all the time.'}
      </p>
      {hasFilters && (
        <Button variant="outline" className="mt-4" onClick={onClear}>
          Clear all filters
        </Button>
      )}
    </div>
  );
}

export function ErrorState({ message, onRetry }: { message: string; onRetry?: () => void }) {
  return (
    <div
      role="alert"
      className="flex flex-wrap items-start gap-3 rounded-2xl border border-danger/40 bg-danger-soft px-4 py-3"
    >
      <TriangleAlert className="mt-0.5 size-4 shrink-0 text-danger" />
      <div className="min-w-0">
        <p className="text-sm font-semibold text-danger">Couldn&apos;t load listings</p>
        <p className="mt-0.5 text-sm text-ink-2">{message}</p>
      </div>
      {onRetry && (
        <Button variant="outline" size="sm" className="ml-auto" onClick={onRetry}>
          Try again
        </Button>
      )}
    </div>
  );
}

export function OwnerCallout() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="mt-10 flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-line bg-surface px-5 py-4 shadow-sm">
      <div>
        <p className="text-[15px] font-semibold text-ink">Own a backroom, cottage or house?</p>
        <p className="text-sm text-ink-2">
          List it free and deal with tenants directly — no agents, no commission.
        </p>
      </div>
      <Button variant="cta" asChild>
        <Link to={isAuthenticated ? paths.manager.newProperty : paths.auth.register}>
          List your property
        </Link>
      </Button>
    </div>
  );
}
