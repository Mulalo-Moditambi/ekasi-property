import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { LayoutGrid, Rows3 } from 'lucide-react';
import { ActiveFilters } from '../features/properties/components/ActiveFilters';
import { FilterPanel } from '../features/properties/components/FilterPanel';
import { SearchBar } from '../features/properties/components/SearchBar';
import { BrowseHero } from '../features/properties/components/browse/BrowseHero';
import { Pager } from '../features/properties/components/browse/Pager';
import { ResultsList } from '../features/properties/components/browse/ResultsList';
import {
  EmptyState,
  ErrorState,
  OwnerCallout,
  ResultsSkeleton,
} from '../features/properties/components/browse/ResultsStates';
import { useSearchProperties } from '../features/properties/queries';
import { browseCodec, countActiveFilters } from '../features/properties/searchParams';
import type { BrowseState, BrowseView } from '../features/properties/searchParams';
import { propertySortLabels } from '../features/properties/types';
import type { PropertySort, PropertySummary, SearchFilters } from '../features/properties/types';
import { errorMessage } from '../shared/api/queryClient';
import { useUrlState } from '../shared/hooks/useUrlState';
import { Button } from '../shared/ui/button';
import { Segmented, SegmentedItem } from '../shared/ui/segmented';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../shared/ui/select';
import { Sheet, SheetContent, SheetTitle } from '../shared/ui/sheet';
import { Skeleton } from '../shared/ui/skeleton';

const PAGE_SIZE = 12;

/**
 * The marketplace. Every piece of state a visitor can change — filters, sort,
 * page, view mode — lives in the query string, so the page itself holds only
 * genuinely local UI state (whether the mobile filter sheet is open).
 */
export function BrowsePage() {
  const { state, setState } = useUrlState<BrowseState>(browseCodec);
  const { filters, sort, page, view } = state;

  const [filtersOpen, setFiltersOpen] = useState(false);

  const { data: result, isPending, isFetching, error, refetch } = useSearchProperties({
    filters,
    page,
    pageSize: PAGE_SIZE,
    sort,
  });

  /**
   * Any filter change resets to page 1 — page 4 of the old result set is
   * meaningless against a new one. `replace` on filter edits keeps the Back
   * button stepping between searches the user meant, not every slider tick.
   */
  const commit = useCallback(
    (next: Partial<BrowseState>, options?: { replace?: boolean }) => {
      setState((current) => ({ ...current, page: 1, ...next }), options);
    },
    [setState],
  );

  const applyFilters = useCallback(
    (next: SearchFilters) => commit({ filters: next }, { replace: true }),
    [commit],
  );

  const clearFilters = useCallback(() => applyFilters({}), [applyFilters]);

  const changeSort = useCallback((value: string) => commit({ sort: value as PropertySort }), [commit]);

  const changeView = useCallback(
    // A view toggle is not a new search — it shouldn't cost a history entry.
    (value: string) => setState((current) => ({ ...current, view: value as BrowseView }), { replace: true }),
    [setState],
  );

  const goToPage = useCallback(
    (next: number) => {
      setState((current) => ({ ...current, page: next }));
      window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    [setState],
  );

  const searchRef = useRef<HTMLDivElement>(null);

  // "/" focuses the location field, the way every search-first tool behaves.
  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      const target = event.target as HTMLElement | null;
      const typingElsewhere =
        target &&
        (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable);

      if (event.key === '/' && !typingElsewhere) {
        event.preventDefault();
        searchRef.current?.querySelector<HTMLInputElement>('input[type="text"]')?.focus();
      }
    }

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, []);

  const activeCount = countActiveFilters(filters);
  const totalCount = result?.totalCount ?? 0;
  const totalPages = result ? Math.max(1, Math.ceil(result.totalCount / result.pageSize)) : 1;
  const firstOnPage = result && totalCount > 0 ? (result.page - 1) * result.pageSize + 1 : 0;
  const lastOnPage = result ? Math.min(result.page * result.pageSize, totalCount) : 0;

  // Suggestions come from the live result set, so every option returns listings.
  const suggestions = useMemo(
    () => [...new Set((result?.items ?? []).flatMap((p) => [p.township, p.city]))].sort(),
    [result],
  );

  return (
    <div>
      <BrowseHero />

      <div className="mx-auto max-w-[1400px] px-4 sm:px-6">
        <div ref={searchRef} className="sticky top-16 z-30 -mt-11 pb-5">
          <SearchBar
            filters={filters}
            onChange={applyFilters}
            suggestions={suggestions}
            activeFilterCount={activeCount}
            onOpenFilters={() => setFiltersOpen(true)}
          />
        </div>

        <div className="pb-4 lg:grid lg:grid-cols-[272px_minmax(0,1fr)] lg:gap-8">
          <aside className="hidden lg:block">
            <div className="sticky top-40 max-h-[calc(100vh-11rem)] overflow-y-auto rounded-2xl border border-line bg-surface shadow-sm">
              <FilterPanel filters={filters} onChange={applyFilters} />
            </div>
          </aside>

          <section className="min-w-0" aria-label="Search results">
            <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
              <div className="min-w-0">
                {isPending ? (
                  <Skeleton className="h-5 w-44" />
                ) : (
                  // Announced politely so filter changes reach screen readers,
                  // which otherwise get no signal that the list moved.
                  <p className="text-sm text-ink-2" aria-live="polite" aria-atomic="true">
                    <span className="tnum font-semibold text-ink">{totalCount}</span>{' '}
                    {totalCount === 1 ? 'home' : 'homes'}
                    {totalCount > 0 && (
                      <span className="tnum text-ink-3">
                        {' '}
                        · showing {firstOnPage}–{lastOnPage}
                      </span>
                    )}
                  </p>
                )}
              </div>

              <div className="flex items-center gap-2">
                <Select value={sort} onValueChange={changeSort}>
                  <SelectTrigger aria-label="Sort listings" className="h-9 w-[10rem] rounded-lg">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(propertySortLabels).map(([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>

                <Segmented value={view} onValueChange={changeView}>
                  <SegmentedItem value="grid" aria-label="Grid view">
                    <LayoutGrid />
                  </SegmentedItem>
                  <SegmentedItem value="rows" aria-label="List view">
                    <Rows3 />
                  </SegmentedItem>
                </Segmented>
              </div>
            </div>

            {activeCount > 0 && (
              <div className="mb-4">
                <ActiveFilters filters={filters} onChange={applyFilters} />
              </div>
            )}

            <Results
              error={error}
              isPending={isPending}
              isRefreshing={isFetching && !isPending}
              items={result?.items ?? []}
              view={view}
              hasFilters={activeCount > 0}
              onClear={clearFilters}
              onRetry={() => void refetch()}
              page={result?.page ?? page}
              totalPages={totalPages}
              onPageChange={goToPage}
            />

            <OwnerCallout />
          </section>
        </div>
      </div>

      {/* Bottom-sheet drawer on mobile, per the spec's mobile pattern. */}
      <Sheet open={filtersOpen} onOpenChange={setFiltersOpen}>
        <SheetContent side="bottom" className="lg:hidden">
          <div className="flex h-12 shrink-0 items-center justify-between border-b border-line pl-4 pr-12">
            <SheetTitle className="text-sm font-semibold text-ink">Filters</SheetTitle>
            {activeCount > 0 && (
              <Button variant="link" size="sm" className="h-auto p-0 text-xs" onClick={clearFilters}>
                Clear all
              </Button>
            )}
          </div>
          <div className="flex-1 overflow-y-auto">
            <FilterPanel filters={filters} onChange={applyFilters} hideHeader showListingType />
          </div>
          <div className="border-t border-line p-3">
            <Button variant="cta" className="w-full" size="lg" onClick={() => setFiltersOpen(false)}>
              Show {totalCount} {totalCount === 1 ? 'home' : 'homes'}
            </Button>
          </div>
        </SheetContent>
      </Sheet>
    </div>
  );
}

interface ResultsProps {
  error: unknown;
  isPending: boolean;
  isRefreshing: boolean;
  items: PropertySummary[];
  view: BrowseView;
  hasFilters: boolean;
  onClear: () => void;
  onRetry: () => void;
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

/** The one place that decides which of the four result states is on screen. */
function Results({
  error,
  isPending,
  isRefreshing,
  items,
  view,
  hasFilters,
  onClear,
  onRetry,
  page,
  totalPages,
  onPageChange,
}: ResultsProps) {
  if (error) {
    return <ErrorState message={errorMessage(error, 'Search failed')} onRetry={onRetry} />;
  }

  if (isPending) {
    return <ResultsSkeleton view={view} />;
  }

  if (items.length === 0) {
    return <EmptyState hasFilters={hasFilters} onClear={onClear} />;
  }

  return (
    <>
      <ResultsList items={items} view={view} isRefreshing={isRefreshing} />
      <Pager page={page} totalPages={totalPages} onChange={onPageChange} />
    </>
  );
}
