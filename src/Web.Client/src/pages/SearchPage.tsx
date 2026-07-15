import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { searchProperties } from '../api/properties';
import { PropertyCard } from '../components/PropertyCard';
import type { PagedResult, PropertySummary, SearchFilters } from '../types/property';
import { ListingType, PropertyType, propertyTypeLabels } from '../types/property';

const PAGE_SIZE = 12;

type ViewMode = 'grid' | 'list';

const VIEW_KEY = 'ekasi.view';

export function SearchPage() {
  const [filters, setFilters] = useState<SearchFilters>({});
  const [result, setResult] = useState<PagedResult<PropertySummary> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [view, setView] = useState<ViewMode>(() =>
    localStorage.getItem(VIEW_KEY) === 'list' ? 'list' : 'grid',
  );

  function switchView(mode: ViewMode) {
    setView(mode);
    localStorage.setItem(VIEW_KEY, mode);
  }

  async function runSearch(activeFilters: SearchFilters, page: number) {
    setLoading(true);
    setError(null);

    try {
      setResult(await searchProperties(activeFilters, page, PAGE_SIZE));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void runSearch({}, 1);
  }, []);

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    void runSearch(filters, 1);
  }

  function goToPage(page: number) {
    void runSearch(filters, page);
    window.scrollTo({ top: 0 });
  }

  const totalPages = result ? Math.max(1, Math.ceil(result.totalCount / result.pageSize)) : 1;

  return (
    <div className="page">
      <section className="hero">
        <h1>Find your next place, ekasi.</h1>
        <p>Backrooms and cottages to rent. Houses to buy. Straight from the owners.</p>
      </section>

      <form className="filter-bar" onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Township (e.g. Orlando West)"
          value={filters.township ?? ''}
          onChange={(e) => setFilters({ ...filters, township: e.target.value || undefined })}
        />
        <select
          value={filters.listingType ?? ''}
          onChange={(e) =>
            setFilters({
              ...filters,
              listingType: e.target.value === '' ? undefined : (Number(e.target.value) as ListingType),
            })
          }
        >
          <option value="">Rent or Buy</option>
          <option value={ListingType.Rent}>To Rent</option>
          <option value={ListingType.Sale}>For Sale</option>
        </select>
        <select
          value={filters.propertyType ?? ''}
          onChange={(e) =>
            setFilters({
              ...filters,
              propertyType: e.target.value === '' ? undefined : (Number(e.target.value) as PropertyType),
            })
          }
        >
          <option value="">Any type</option>
          {Object.entries(propertyTypeLabels).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
        <input
          type="number"
          min="0"
          placeholder="Min price"
          value={filters.minPrice ?? ''}
          onChange={(e) =>
            setFilters({ ...filters, minPrice: e.target.value === '' ? undefined : Number(e.target.value) })
          }
        />
        <input
          type="number"
          min="0"
          placeholder="Max price"
          value={filters.maxPrice ?? ''}
          onChange={(e) =>
            setFilters({ ...filters, maxPrice: e.target.value === '' ? undefined : Number(e.target.value) })
          }
        />
        <select
          value={filters.minBedrooms ?? ''}
          onChange={(e) =>
            setFilters({
              ...filters,
              minBedrooms: e.target.value === '' ? undefined : Number(e.target.value),
            })
          }
        >
          <option value="">Any beds</option>
          <option value="1">1+</option>
          <option value="2">2+</option>
          <option value="3">3+</option>
          <option value="4">4+</option>
        </select>
        <button type="submit">Search</button>
      </form>

      {error && <p className="error">{error}</p>}
      {loading ? (
        <p className="muted">Loading listings…</p>
      ) : !result || result.items.length === 0 ? (
        <p className="muted">No listings match your search yet. Try widening the filters.</p>
      ) : (
        <>
          <div className="results-toolbar">
            <p className="muted result-count">
              {result.totalCount} listing{result.totalCount === 1 ? '' : 's'} found
            </p>
            <div className="view-toggle" role="group" aria-label="View mode">
              <button
                type="button"
                className={view === 'grid' ? 'active' : ''}
                onClick={() => switchView('grid')}
              >
                ▦ Grid
              </button>
              <button
                type="button"
                className={view === 'list' ? 'active' : ''}
                onClick={() => switchView('list')}
              >
                ☰ List
              </button>
            </div>
          </div>
          <div className={view === 'grid' ? 'card-grid' : 'card-list'}>
            {result.items.map((property) => (
              <PropertyCard key={property.id} property={property} />
            ))}
          </div>
          {totalPages > 1 && (
            <div className="pagination">
              <button type="button" disabled={result.page <= 1 || loading} onClick={() => goToPage(result.page - 1)}>
                ‹ Previous
              </button>
              <span className="muted">
                Page {result.page} of {totalPages}
              </span>
              <button type="button" disabled={!result.hasNextPage || loading} onClick={() => goToPage(result.page + 1)}>
                Next ›
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
