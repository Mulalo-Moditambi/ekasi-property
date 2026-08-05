import type { PropertySort, SearchFilters } from './types';
import { ListingType, PropertyType } from './types';

/**
 * The browse page keeps its entire state in the query string, so a search is a
 * link: shareable, bookmarkable, and navigable with the browser's back button.
 * Keys are kept short and readable because users see them.
 */
export interface BrowseState {
  filters: SearchFilters;
  sort: PropertySort;
  page: number;
  view: BrowseView;
}

/** Grid of cards vs. dense rows. In the URL because a shared link should look the same. */
export type BrowseView = 'grid' | 'rows';

const VIEWS: BrowseView[] = ['grid', 'rows'];

export const DEFAULT_VIEW: BrowseView = 'grid';

export const AMENITY_KEYS = ['hasElectricity', 'waterIncluded', 'hasOwnEntrance', 'hasParking'] as const;

export type AmenityKey = (typeof AMENITY_KEYS)[number];

/** Short URL tokens for the amenity flags — `?has=power,water`. */
const AMENITY_TOKENS: Record<AmenityKey, string> = {
  hasElectricity: 'power',
  waterIncluded: 'water',
  hasOwnEntrance: 'entrance',
  hasParking: 'parking',
};

export const AMENITY_LABELS: Record<AmenityKey, string> = {
  hasElectricity: 'Electricity',
  waterIncluded: 'Water included',
  hasOwnEntrance: 'Own entrance',
  hasParking: 'Parking',
};

const SORTS: PropertySort[] = ['newest', 'price_asc', 'price_desc'];

function toPositiveInt(raw: string | null): number | undefined {
  if (raw === null || raw.trim() === '') return undefined;
  const value = Number(raw);
  return Number.isFinite(value) && value >= 0 ? Math.floor(value) : undefined;
}

/** Only accept values that are actually members of the enum — a hand-edited URL shouldn't crash the page. */
function toEnum<T extends number>(raw: string | null, enumObject: Record<string, unknown>): T | undefined {
  const value = toPositiveInt(raw);
  return value !== undefined && enumObject[value] !== undefined ? (value as T) : undefined;
}

export function parseBrowseState(params: URLSearchParams): BrowseState {
  const amenityTokens = new Set((params.get('has') ?? '').split(',').filter(Boolean));

  const filters: SearchFilters = {
    township: params.get('q')?.trim() || undefined,
    listingType: toEnum<ListingType>(params.get('for'), ListingType),
    propertyType: toEnum<PropertyType>(params.get('kind'), PropertyType),
    minPrice: toPositiveInt(params.get('min')),
    maxPrice: toPositiveInt(params.get('max')),
    minBedrooms: toPositiveInt(params.get('beds')),
  };

  for (const key of AMENITY_KEYS) {
    if (amenityTokens.has(AMENITY_TOKENS[key])) {
      filters[key] = true;
    }
  }

  const sort = params.get('sort') as PropertySort | null;
  const view = params.get('view') as BrowseView | null;

  return {
    filters,
    sort: sort && SORTS.includes(sort) ? sort : 'newest',
    page: Math.max(1, toPositiveInt(params.get('page')) ?? 1),
    view: view && VIEWS.includes(view) ? view : DEFAULT_VIEW,
  };
}

export function browseStateToParams({ filters, sort, page, view }: BrowseState): URLSearchParams {
  const params = new URLSearchParams();

  if (filters.township) params.set('q', filters.township);
  if (filters.listingType !== undefined) params.set('for', String(filters.listingType));
  if (filters.propertyType !== undefined) params.set('kind', String(filters.propertyType));
  if (filters.minPrice !== undefined) params.set('min', String(filters.minPrice));
  if (filters.maxPrice !== undefined) params.set('max', String(filters.maxPrice));
  if (filters.minBedrooms !== undefined) params.set('beds', String(filters.minBedrooms));

  const amenities = AMENITY_KEYS.filter((key) => filters[key]).map((key) => AMENITY_TOKENS[key]);
  if (amenities.length > 0) params.set('has', amenities.join(','));

  // Defaults stay out of the URL so a plain `/` is the canonical empty search.
  if (sort !== 'newest') params.set('sort', sort);
  if (page > 1) params.set('page', String(page));
  if (view !== DEFAULT_VIEW) params.set('view', view);

  return params;
}

/** The codec `useUrlState` needs — module-level so its identity is stable. */
export const browseCodec = {
  parse: parseBrowseState,
  serialize: browseStateToParams,
};

export function countActiveFilters(filters: SearchFilters): number {
  return Object.values(filters).filter((value) => value !== undefined && value !== '').length;
}
