import { useEffect, useRef, useState } from 'react';
import { Check } from 'lucide-react';
import { Button } from '../../../shared/ui/button';
import { Checkbox } from '../../../shared/ui/checkbox';
import { FilterGroup } from '../../../shared/ui/field';
import { Input } from '../../../shared/ui/input';
import { Segmented, SegmentedItem } from '../../../shared/ui/segmented';
import { cn } from '../../../shared/ui/utils';
import { AMENITY_KEYS, AMENITY_LABELS, countActiveFilters } from '../searchParams';
import type { SearchFilters } from '../types';
import { ListingType, PropertyType, propertyTypeLabels } from '../types';

interface FilterPanelProps {
  filters: SearchFilters;
  onChange: (filters: SearchFilters) => void;
  /** The mobile sheet supplies its own title bar, so the built-in header is suppressed there. */
  hideHeader?: boolean;
  /**
   * Buy/Rent normally lives in the search bar, but that collapses on mobile —
   * the sheet re-homes it so every filter stays reachable.
   */
  showListingType?: boolean;
}

const BEDROOM_CHOICES = [1, 2, 3, 4];

function toPrice(raw: string): number | undefined {
  const value = Number(raw.replace(/\s/g, ''));
  return raw.trim() === '' || !Number.isFinite(value) || value < 0 ? undefined : value;
}

/**
 * The filter rail. Every control commits immediately — there is no "Search"
 * button, because the result count in the toolbar is the feedback. Only the
 * free-typed price fields are debounced, so typing "1500" fires one request
 * rather than four.
 */
export function FilterPanel({
  filters,
  onChange,
  hideHeader = false,
  showListingType = false,
}: FilterPanelProps) {
  const [prices, setPrices] = useState({
    min: filters.minPrice?.toString() ?? '',
    max: filters.maxPrice?.toString() ?? '',
  });
  // Distinguishes "the user typed" from "the URL changed under us" so that
  // clearing a chip doesn't immediately get re-committed by the debounce.
  const editing = useRef(false);

  useEffect(() => {
    editing.current = false;
    setPrices({ min: filters.minPrice?.toString() ?? '', max: filters.maxPrice?.toString() ?? '' });
  }, [filters.minPrice, filters.maxPrice]);

  useEffect(() => {
    if (!editing.current) return;

    const timer = setTimeout(() => {
      editing.current = false;
      onChange({ ...filters, minPrice: toPrice(prices.min), maxPrice: toPrice(prices.max) });
    }, 400);

    return () => clearTimeout(timer);
  }, [prices, filters, onChange]);

  function patch(changes: Partial<SearchFilters>) {
    onChange({ ...filters, ...changes });
  }

  function editPrice(changes: Partial<typeof prices>) {
    editing.current = true;
    setPrices((current) => ({ ...current, ...changes }));
  }

  const activeCount = countActiveFilters(filters);

  return (
    <div className="divide-y divide-line">
      {/* Rent vs Buy is the first decision a visitor makes, so it lives in the
          command bar rather than down here in the rail. */}
      {!hideHeader && (
        <div className="flex items-center justify-between gap-2 px-4 py-3">
          <h2 className="flex items-center gap-1.5 text-sm font-semibold text-ink">
            Filters
            {activeCount > 0 && (
              <span className="tnum grid size-[18px] place-items-center rounded-full bg-brand-bright text-2xs font-bold text-brand-fg">
                {activeCount}
              </span>
            )}
          </h2>
          {activeCount > 0 && (
            <Button variant="link" size="sm" className="h-auto p-0 text-xs" onClick={() => onChange({})}>
              Clear all
            </Button>
          )}
        </div>
      )}

      {showListingType && (
        <FilterGroup label="Buying or renting">
          <Segmented
            className="w-full"
            value={filters.listingType === undefined ? 'any' : String(filters.listingType)}
            onValueChange={(value) =>
              patch({ listingType: value === 'any' ? undefined : (Number(value) as ListingType) })
            }
          >
            <SegmentedItem value="any" className="flex-1">
              All
            </SegmentedItem>
            <SegmentedItem value={String(ListingType.Sale)} className="flex-1">
              Buy
            </SegmentedItem>
            <SegmentedItem value={String(ListingType.Rent)} className="flex-1">
              Rent
            </SegmentedItem>
          </Segmented>
        </FilterGroup>
      )}

      <FilterGroup label="Property type">
        <div role="radiogroup" aria-label="Property type" className="-mx-1">
          <TypeChoice
            label="Any type"
            selected={filters.propertyType === undefined}
            onSelect={() => patch({ propertyType: undefined })}
          />
          {Object.entries(propertyTypeLabels).map(([value, label]) => {
            const propertyType = Number(value) as PropertyType;
            return (
              <TypeChoice
                key={value}
                label={label}
                selected={filters.propertyType === propertyType}
                onSelect={() => patch({ propertyType })}
              />
            );
          })}
        </div>
      </FilterGroup>

      <FilterGroup label="Price">
        <div className="flex items-center gap-2">
          <Input
            type="number"
            min="0"
            inputMode="numeric"
            aria-label="Minimum price"
            placeholder="Min"
            className="tnum"
            value={prices.min}
            onChange={(e) => editPrice({ min: e.target.value })}
          />
          <span className="text-xs text-ink-3">to</span>
          <Input
            type="number"
            min="0"
            inputMode="numeric"
            aria-label="Maximum price"
            placeholder="Max"
            className="tnum"
            value={prices.max}
            onChange={(e) => editPrice({ max: e.target.value })}
          />
        </div>
      </FilterGroup>

      <FilterGroup label="Bedrooms">
        <Segmented
          className="w-full"
          value={filters.minBedrooms === undefined ? 'any' : String(filters.minBedrooms)}
          onValueChange={(value) =>
            patch({ minBedrooms: value === 'any' ? undefined : Number(value) })
          }
        >
          <SegmentedItem value="any" className="flex-1">
            Any
          </SegmentedItem>
          {BEDROOM_CHOICES.map((count) => (
            <SegmentedItem key={count} value={String(count)} className="flex-1 tnum">
              {count}+
            </SegmentedItem>
          ))}
        </Segmented>
      </FilterGroup>

      <FilterGroup label="Must have">
        <div className="space-y-2.5">
          {AMENITY_KEYS.map((key) => (
            <label key={key} className="flex cursor-pointer items-center gap-2.5 text-sm text-ink">
              <Checkbox
                checked={filters[key] ?? false}
                onCheckedChange={(checked) => patch({ [key]: checked ? true : undefined })}
              />
              {AMENITY_LABELS[key]}
            </label>
          ))}
        </div>
      </FilterGroup>
    </div>
  );
}

function TypeChoice({
  label,
  selected,
  onSelect,
}: {
  label: string;
  selected: boolean;
  onSelect: () => void;
}) {
  return (
    <button
      type="button"
      role="radio"
      aria-checked={selected}
      onClick={onSelect}
      className={cn(
        'flex w-full items-center justify-between rounded-md px-1 py-1.5 text-sm transition-colors outline-none',
        'hover:bg-surface-3 focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-brand',
        selected ? 'font-medium text-ink' : 'text-ink-2',
      )}
    >
      {label}
      {selected && <Check className="size-3.5 text-brand" strokeWidth={3} />}
    </button>
  );
}
