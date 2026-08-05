import { memo } from 'react';
import { cn } from '../../../../shared/ui/utils';
import { ListingRow } from '../ListingRow';
import { PropertyCard } from '../PropertyCard';
import type { BrowseView } from '../../searchParams';
import type { PropertySummary } from '../../types';

interface ResultsListProps {
  items: PropertySummary[];
  view: BrowseView;
  /** True while a background refetch replaces the currently-shown page. */
  isRefreshing?: boolean;
}

/**
 * Pure presentation: it receives listings and a view mode and renders one of the
 * two layouts. It holds no query, no filter and no URL knowledge, which is what
 * lets it be memoised — the page around it re-renders far more often than the
 * result set actually changes.
 */
export const ResultsList = memo(function ResultsList({ items, view, isRefreshing }: ResultsListProps) {
  return (
    <div
      // Keeping the stale page visible but dimmed reads as "updating" rather
      // than the whole column collapsing into skeletons on every filter change.
      aria-busy={isRefreshing}
      className={cn(
        'transition-opacity duration-200',
        isRefreshing && 'opacity-60',
        view === 'grid' ? 'grid gap-5 sm:grid-cols-2 xl:grid-cols-3' : 'flex flex-col gap-3',
      )}
    >
      {items.map((property) =>
        view === 'grid' ? (
          <PropertyCard key={property.id} property={property} />
        ) : (
          <ListingRow key={property.id} property={property} />
        ),
      )}
    </div>
  );
});
