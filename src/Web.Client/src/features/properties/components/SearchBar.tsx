import { useCallback, useEffect, useId, useState } from 'react';
import { ChevronDown, MapPin, Search, SlidersHorizontal } from 'lucide-react';
import { useDebouncedInput } from '../../../shared/hooks/useUrlState';
import { Button } from '../../../shared/ui/button';
import { Input } from '../../../shared/ui/input';
import { Popover, PopoverContent, PopoverTrigger } from '../../../shared/ui/popover';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../../shared/ui/select';
import { cn } from '../../../shared/ui/utils';
import type { SearchFilters } from '../types';
import { ListingType, PropertyType, propertyTypeLabels } from '../types';

interface SearchBarProps {
  filters: SearchFilters;
  onChange: (filters: SearchFilters) => void;
  /** Townships present in the current results — real values, so every suggestion returns something. */
  suggestions: string[];
  activeFilterCount: number;
  onOpenFilters: () => void;
  className?: string;
}

const zar = new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR', maximumFractionDigits: 0 });

function toPrice(raw: string): number | undefined {
  const value = Number(raw.replace(/\s/g, ''));
  return raw.trim() === '' || !Number.isFinite(value) || value < 0 ? undefined : value;
}

function priceLabel(min?: number, max?: number): string {
  if (min === undefined && max === undefined) return 'Any price';
  if (min !== undefined && max !== undefined) return `${zar.format(min)} – ${zar.format(max)}`;
  return min !== undefined ? `From ${zar.format(min)}` : `Up to ${zar.format(max!)}`;
}

/**
 * The floating command unit: intent pills, location, price and type, plus the
 * search action. Filters commit as they change — the CTA is there to satisfy the
 * expectation that a search bar has a search button, and to flush a half-typed
 * location without waiting on the debounce.
 */
export function SearchBar({
  filters,
  onChange,
  suggestions,
  activeFilterCount,
  onOpenFilters,
  className,
}: SearchBarProps) {
  const listId = useId();
  const [prices, setPrices] = useState({
    min: filters.minPrice?.toString() ?? '',
    max: filters.maxPrice?.toString() ?? '',
  });

  useEffect(() => {
    setPrices({ min: filters.minPrice?.toString() ?? '', max: filters.maxPrice?.toString() ?? '' });
  }, [filters.minPrice, filters.maxPrice]);

  const commitTownship = useCallback(
    (next: string) => onChange({ ...filters, township: next.trim() || undefined }),
    [filters, onChange],
  );

  // Typing stays local until it settles; the shared hook owns the debounce so
  // the search field and any future filter input behave identically.
  const [location, setLocation] = useDebouncedInput(filters.township ?? '', commitTownship, 400);

  /** Enter and the search button skip the wait. */
  const commitLocation = useCallback(() => commitTownship(location), [commitTownship, location]);

  return (
    <div
      className={cn(
        'rounded-2xl border border-line/80 bg-surface/90 p-2 shadow-xl backdrop-blur-md',
        className,
      )}
    >
      {/*
        One row at every width. On mobile everything except the location field
        collapses into the filter sheet, so the sticky bar stays ~56px tall and
        doesn't eat the viewport while scrolling results.
      */}
      <div className="flex items-center gap-2">
        {/* Buy / Rent intent pills */}
        <div
          className="hidden shrink-0 items-center gap-1 rounded-full bg-surface-3 p-1 lg:flex"
          role="group"
          aria-label="Listing type"
        >
          {[
            { value: undefined, label: 'All' },
            { value: ListingType.Sale, label: 'Buy' },
            { value: ListingType.Rent, label: 'Rent' },
          ].map((pill) => {
            const active = filters.listingType === pill.value;
            return (
              <button
                key={pill.label}
                type="button"
                aria-pressed={active}
                onClick={() => onChange({ ...filters, listingType: pill.value })}
                className={cn(
                  'rounded-full px-4 py-1.5 text-sm font-medium outline-none',
                  'transition-[background-color,color,box-shadow] duration-200 ease-[cubic-bezier(0.4,0,0.2,1)]',
                  'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta',
                  active ? 'bg-brand text-brand-fg shadow-sm' : 'text-ink-2 hover:text-ink',
                )}
              >
                {pill.label}
              </button>
            );
          })}
        </div>

        <span className="hidden h-8 w-px shrink-0 bg-line lg:block" aria-hidden="true" />

        {/* Location, with suggestions drawn from the live result set */}
        <div className="relative min-w-0 flex-1">
          <MapPin className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-ink-3" />
          <Input
            type="text"
            list={listId}
            value={location}
            onChange={(e) => setLocation(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && commitLocation()}
            aria-label="Location"
            placeholder="Township or city"
            className="h-11 rounded-xl border-transparent bg-transparent pl-9 text-[15px] shadow-none focus-visible:border-line"
          />
          <datalist id={listId}>
            {suggestions.map((township) => (
              <option key={township} value={township} />
            ))}
          </datalist>
        </div>

        <span className="hidden h-8 w-px shrink-0 bg-line lg:block" aria-hidden="true" />

        {/* Price range */}
        <Popover>
          <PopoverTrigger asChild>
            <button
              type="button"
              className={cn(
                'hidden h-11 shrink-0 items-center justify-between gap-2 rounded-xl px-3 text-[15px] outline-none lg:flex',
                'transition-colors duration-200 hover:bg-surface-3',
                'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta',
                filters.minPrice === undefined && filters.maxPrice === undefined ? 'text-ink-3' : 'text-ink',
              )}
            >
              <span className="tnum truncate">{priceLabel(filters.minPrice, filters.maxPrice)}</span>
              <ChevronDown className="size-4 shrink-0 text-ink-3" />
            </button>
          </PopoverTrigger>
          <PopoverContent className="w-72">
            <p className="mb-3 text-sm font-semibold text-ink">Price range</p>
            <div className="flex items-center gap-2">
              <Input
                type="number"
                min="0"
                inputMode="numeric"
                aria-label="Minimum price"
                placeholder="No min"
                className="tnum"
                value={prices.min}
                onChange={(e) => setPrices((p) => ({ ...p, min: e.target.value }))}
              />
              <span className="text-sm text-ink-3">to</span>
              <Input
                type="number"
                min="0"
                inputMode="numeric"
                aria-label="Maximum price"
                placeholder="No max"
                className="tnum"
                value={prices.max}
                onChange={(e) => setPrices((p) => ({ ...p, max: e.target.value }))}
              />
            </div>
            <div className="mt-3 flex justify-end gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setPrices({ min: '', max: '' });
                  onChange({ ...filters, minPrice: undefined, maxPrice: undefined });
                }}
              >
                Clear
              </Button>
              <Button
                variant="cta"
                size="sm"
                onClick={() =>
                  onChange({ ...filters, minPrice: toPrice(prices.min), maxPrice: toPrice(prices.max) })
                }
              >
                Apply
              </Button>
            </div>
          </PopoverContent>
        </Popover>

        <span className="hidden h-8 w-px shrink-0 bg-line lg:block" aria-hidden="true" />

        {/* Property type */}
        <Select
          value={filters.propertyType === undefined ? 'any' : String(filters.propertyType)}
          onValueChange={(value) =>
            onChange({
              ...filters,
              propertyType: value === 'any' ? undefined : (Number(value) as PropertyType),
            })
          }
        >
          <SelectTrigger
            aria-label="Property type"
            className="hidden h-11 shrink-0 rounded-xl border-transparent bg-transparent px-3 text-[15px] hover:bg-surface-3 lg:flex lg:w-[9.5rem]"
          >
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="any">Any type</SelectItem>
            {Object.entries(propertyTypeLabels).map(([value, label]) => (
              <SelectItem key={value} value={value}>
                {label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <div className="flex shrink-0 items-center gap-2">
          <Button
            variant="outline"
            size="lg"
            aria-label="Open filters"
            className="h-11 rounded-xl px-3 lg:hidden"
            onClick={onOpenFilters}
          >
            <SlidersHorizontal />
            {activeFilterCount > 0 && (
              <span className="tnum grid size-5 place-items-center rounded-full bg-brand text-2xs font-bold text-brand-fg">
                {activeFilterCount}
              </span>
            )}
          </Button>
          <Button
            variant="cta"
            size="lg"
            aria-label="Search"
            className="h-11 rounded-xl px-3.5 lg:px-5"
            onClick={commitLocation}
          >
            <Search />
            <span className="sr-only xl:not-sr-only">Search</span>
          </Button>
        </div>
      </div>
    </div>
  );
}
