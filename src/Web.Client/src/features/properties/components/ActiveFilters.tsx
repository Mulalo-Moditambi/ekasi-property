import { X } from 'lucide-react';
import { AMENITY_KEYS, AMENITY_LABELS } from '../searchParams';
import type { SearchFilters } from '../types';
import { listingTypeLabels, propertyTypeLabels } from '../types';

interface ActiveFiltersProps {
  filters: SearchFilters;
  onChange: (filters: SearchFilters) => void;
}

const zar = new Intl.NumberFormat('en-ZA', {
  style: 'currency',
  currency: 'ZAR',
  maximumFractionDigits: 0,
});

/**
 * Every applied filter, restated as a dismissible chip. In a rail this dense it
 * is otherwise easy to forget why a search returns four results — and easy to
 * lose the one filter you want to drop.
 */
export function ActiveFilters({ filters, onChange }: ActiveFiltersProps) {
  const chips: { key: string; label: string; clear: () => void }[] = [];
  const without = (changes: Partial<SearchFilters>) => () => onChange({ ...filters, ...changes });

  if (filters.township) {
    chips.push({ key: 'township', label: `“${filters.township}”`, clear: without({ township: undefined }) });
  }

  if (filters.listingType !== undefined) {
    chips.push({
      key: 'listingType',
      label: listingTypeLabels[filters.listingType],
      clear: without({ listingType: undefined }),
    });
  }

  if (filters.propertyType !== undefined) {
    chips.push({
      key: 'propertyType',
      label: propertyTypeLabels[filters.propertyType],
      clear: without({ propertyType: undefined }),
    });
  }

  if (filters.minPrice !== undefined) {
    chips.push({
      key: 'minPrice',
      label: `From ${zar.format(filters.minPrice)}`,
      clear: without({ minPrice: undefined }),
    });
  }

  if (filters.maxPrice !== undefined) {
    chips.push({
      key: 'maxPrice',
      label: `Up to ${zar.format(filters.maxPrice)}`,
      clear: without({ maxPrice: undefined }),
    });
  }

  if (filters.minBedrooms !== undefined) {
    chips.push({
      key: 'minBedrooms',
      label: `${filters.minBedrooms}+ bed`,
      clear: without({ minBedrooms: undefined }),
    });
  }

  for (const key of AMENITY_KEYS) {
    if (filters[key]) {
      chips.push({ key, label: AMENITY_LABELS[key], clear: without({ [key]: undefined }) });
    }
  }

  if (chips.length === 0) {
    return null;
  }

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {chips.map((chip) => (
        <button
          key={chip.key}
          type="button"
          onClick={chip.clear}
          className="group inline-flex items-center gap-1 rounded-md border border-brand-line bg-brand-soft py-1 pl-2 pr-1.5 text-xs font-medium text-brand outline-none transition-colors hover:bg-brand-bright hover:text-brand-fg focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-brand"
        >
          {chip.label}
          <X className="size-3" />
          <span className="sr-only">Remove filter</span>
        </button>
      ))}
      <button
        type="button"
        onClick={() => onChange({})}
        className="ml-0.5 rounded-md px-1.5 py-1 text-xs font-medium text-ink-3 underline-offset-4 outline-none transition-colors hover:text-ink hover:underline focus-visible:outline-2 focus-visible:outline-brand"
      >
        Clear all
      </button>
    </div>
  );
}
