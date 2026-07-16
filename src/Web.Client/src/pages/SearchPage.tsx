import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext';
import { searchProperties } from '../features/properties/api';
import { PropertyCard } from '../features/properties/components/PropertyCard';
import { PropertyFilters } from '../features/properties/components/PropertyFilters';
import { Pagination } from '../shared/components/Pagination';
import { PARTNERSHIP_PHOTO, STOCK_HOME_PHOTOS } from '../shared/stockPhotos';
import type { PagedResult, PropertySort, PropertySummary, SearchFilters } from '../features/properties/types';
import { propertySortLabels } from '../features/properties/types';

const PAGE_SIZE = 12;

type ViewMode = 'grid' | 'list';

const VIEW_KEY = 'ekasi.view';

const HOW_IT_WORKS = [
  { icon: '🔍', title: 'Search ekasi', text: 'Filter by township, price and rooms to find places near you.' },
  { icon: '💬', title: 'Contact the owner', text: 'Message the owner directly — no agents, no middleman fees.' },
  { icon: '🔑', title: 'Move in', text: 'Arrange a viewing and get the keys to your new place.' },
];

export function SearchPage() {
  const { isAuthenticated } = useAuth();
  const [filters, setFilters] = useState<SearchFilters>({});
  const [sort, setSort] = useState<PropertySort>('newest');
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

  async function runSearch(activeFilters: SearchFilters, page: number, activeSort: PropertySort) {
    setLoading(true);
    setError(null);

    try {
      setResult(await searchProperties(activeFilters, page, PAGE_SIZE, activeSort));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void runSearch({}, 1, 'newest');
  }, []);

  function changeSort(next: PropertySort) {
    setSort(next);
    void runSearch(filters, 1, next);
  }

  function goToPage(page: number) {
    void runSearch(filters, page, sort);
    window.scrollTo({ top: 0 });
  }

  const totalPages = result ? Math.max(1, Math.ceil(result.totalCount / result.pageSize)) : 1;

  // Combine real listing photos with stock photos, ensuring we have 3 reliable images.
  // Stock photos (from Unsplash) are always included as primary fallback.
  const realListingPhotos = (result?.items ?? [])
    .map((p) => p.imageUrls[0])
    .filter((url): url is string => !!url)
    .slice(0, 1);

  const collagePhotos = [
    ...realListingPhotos,
    ...STOCK_HOME_PHOTOS,
  ].slice(0, 3);

  return (
    <div className="page">
      <section className="hero">
        <div className="hero-copy">
          <p className="hero-eyebrow">Township rentals &amp; homes</p>
          <h1>
            Find your next place, <span className="accent">ekasi.</span>
          </h1>
          <p className="hero-lead">
            Backrooms and cottages to rent, houses to buy — listed straight by the owners. No agents, no middleman
            fees. Browse what&apos;s available near you.
          </p>
        </div>
        <div className="hero-collage" aria-hidden="true">
          {collagePhotos.map((url) => (
            <div key={url} className="collage-tile">
              <img src={url} alt="" loading="lazy" />
            </div>
          ))}
        </div>
      </section>

      <div className="search-layout">
        <div className="filter-rail">
          <PropertyFilters
            value={filters}
            onChange={setFilters}
            onSearch={(next) => void runSearch(next, 1, sort)}
          />
        </div>

        <div className="search-results">
          {error && <p className="error">{error}</p>}
          {loading ? (
            <div className="card-grid" aria-hidden="true">
              {Array.from({ length: 6 }, (_, i) => (
                <div key={i} className="skeleton-card">
                  <div className="skeleton skeleton-photo" />
                  <div className="skeleton-body">
                    <div className="skeleton skeleton-line w40" />
                    <div className="skeleton skeleton-line w80" />
                    <div className="skeleton skeleton-line w60" />
                  </div>
                </div>
              ))}
            </div>
          ) : !result || result.items.length === 0 ? (
            <div className="empty-state">
              <span className="empty-icon" aria-hidden="true">
                🏘️
              </span>
              <h2>No listings found</h2>
              <p className="muted">
                Try widening your filters, or check back soon — new places are added all the time.
              </p>
              <button
                type="button"
                onClick={() => {
                  setFilters({});
                  void runSearch({}, 1, sort);
                }}
              >
                Clear filters
              </button>
            </div>
          ) : (
            <div className={view === 'list' ? 'results results-list' : 'results'}>
              <div className="results-toolbar">
                <p className="muted result-count">
                  {result.totalCount} listing{result.totalCount === 1 ? '' : 's'} found
                </p>
                <div className="toolbar-controls">
                  <label className="sort-control">
                    <span className="sr-only">Sort listings</span>
                    <select value={sort} onChange={(e) => changeSort(e.target.value as PropertySort)}>
                      {Object.entries(propertySortLabels).map(([value, label]) => (
                        <option key={value} value={value}>
                          {label}
                        </option>
                      ))}
                    </select>
                  </label>
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
              </div>
              <div className={view === 'grid' ? 'card-grid' : 'card-list'}>
                {result.items.map((property) => (
                  <PropertyCard key={property.id} property={property} />
                ))}
              </div>
              <Pagination page={result.page} totalPages={totalPages} disabled={loading} onPageChange={goToPage} />
            </div>
          )}
        </div>
      </div>

      <section className="how-it-works" aria-label="How Ekasi Property works">
        <h2>How Ekasi Property works</h2>
        <ol className="steps">
          {HOW_IT_WORKS.map((step, index) => (
            <li key={step.title}>
              <span className="step-icon" aria-hidden="true">
                {step.icon}
              </span>
              <span className="step-number">Step {index + 1}</span>
              <h3>{step.title}</h3>
              <p>{step.text}</p>
            </li>
          ))}
        </ol>
      </section>

      <section className="owner-cta">
        <img className="cta-bg" src={PARTNERSHIP_PHOTO} alt="" loading="lazy" />
        <div className="cta-content">
          <h2>Own a backroom, cottage or house?</h2>
          <p>
            List it free and deal directly with tenants and buyers — no agents, no commission. Your place could be
            earning by next week.
          </p>
          <Link to={isAuthenticated ? '/list-property' : '/register'} className="btn-link">
            List your property
          </Link>
        </div>
      </section>
    </div>
  );
}
