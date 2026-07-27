import { useEffect, useRef, useState } from 'react';
import type { FormEvent } from 'react';
import type { SearchFilters } from '../types';
import { ListingType, PropertyType, propertyTypeLabels } from '../types';

interface PropertyFiltersProps {
  value: SearchFilters;
  onChange: (filters: SearchFilters) => void;
  /** Called with the filters to search with — chips apply instantly, the form on submit. */
  onSearch: (filters: SearchFilters) => void;
}

/**
 * The full search unit: property-type chips + field filters grouped as one glass
 * rail on the left of the results. It sticks below the site header while scrolling
 * so filters stay reachable; a zero-height sentinel above it tells us when it is
 * stuck (for elevated styling).
 */
export function PropertyFilters({ value, onChange, onSearch }: PropertyFiltersProps) {
  const [isStuck, setIsStuck] = useState(false);
  const sentinelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(
      ([entry]) => setIsStuck(!entry.isIntersecting),
      { rootMargin: '-58px 0px 0px 0px', threshold: 0 },
    );
    observer.observe(sentinel);

    return () => observer.disconnect();
  }, []);

  function patch(changes: Partial<SearchFilters>) {
    onChange({ ...value, ...changes });
  }

  function pickType(propertyType?: PropertyType) {
    const next = { ...value, propertyType };
    onChange(next);
    onSearch(next);
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    onSearch(value);
  }

  return (
    <>
      <div ref={sentinelRef} className="filter-sentinel" aria-hidden="true" />
      <section className={isStuck ? 'filter-panel is-stuck' : 'filter-panel'} aria-label="Search filters">
        <div className="category-chips" role="group" aria-label="Property type">
          <button
            type="button"
            className={value.propertyType === undefined ? 'chip active' : 'chip'}
            onClick={() => pickType(undefined)}
          >
            All
          </button>
          {Object.entries(propertyTypeLabels).map(([type, label]) => {
            const propertyType = Number(type) as PropertyType;
            return (
              <button
                key={type}
                type="button"
                className={value.propertyType === propertyType ? 'chip active' : 'chip'}
                onClick={() => pickType(propertyType)}
              >
                {label}
              </button>
            );
          })}
        </div>
        <form className="filter-bar" onSubmit={handleSubmit}>
          <input
            type="text"
            aria-label="Township"
            placeholder="Township (e.g. Orlando West)"
            value={value.township ?? ''}
            onChange={(e) => patch({ township: e.target.value || undefined })}
          />
          <select
            aria-label="Listing type"
            value={value.listingType ?? ''}
            onChange={(e) =>
              patch({ listingType: e.target.value === '' ? undefined : (Number(e.target.value) as ListingType) })
            }
          >
            <option value="">Rent or Buy</option>
            <option value={ListingType.Rent}>To Rent</option>
            <option value={ListingType.Sale}>For Sale</option>
          </select>
          <input
            type="number"
            min="0"
            aria-label="Minimum price"
            placeholder="Min price"
            value={value.minPrice ?? ''}
            onChange={(e) => patch({ minPrice: e.target.value === '' ? undefined : Number(e.target.value) })}
          />
          <input
            type="number"
            min="0"
            aria-label="Maximum price"
            placeholder="Max price"
            value={value.maxPrice ?? ''}
            onChange={(e) => patch({ maxPrice: e.target.value === '' ? undefined : Number(e.target.value) })}
          />
          <select
            aria-label="Minimum bedrooms"
            value={value.minBedrooms ?? ''}
            onChange={(e) => patch({ minBedrooms: e.target.value === '' ? undefined : Number(e.target.value) })}
          >
            <option value="">Any beds</option>
            <option value="1">1+</option>
            <option value="2">2+</option>
            <option value="3">3+</option>
            <option value="4">4+</option>
          </select>
          <button type="submit">Search</button>
        </form>
      </section>
    </>
  );
}
