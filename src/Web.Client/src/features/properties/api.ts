import { request } from '../../shared/api/client';
import type {
  CreateListingRequest,
  PagedResult,
  PropertyDetail,
  PropertySort,
  PropertySummary,
  SearchFilters,
} from './types';

export function searchProperties(
  filters: SearchFilters,
  page = 1,
  pageSize = 12,
  sort: PropertySort = 'newest',
): Promise<PagedResult<PropertySummary>> {
  const params = new URLSearchParams();

  if (filters.township) params.set('township', filters.township);
  if (filters.listingType !== undefined) params.set('listingType', String(filters.listingType));
  if (filters.propertyType !== undefined) params.set('propertyType', String(filters.propertyType));
  if (filters.minPrice !== undefined) params.set('minPrice', String(filters.minPrice));
  if (filters.maxPrice !== undefined) params.set('maxPrice', String(filters.maxPrice));
  if (filters.minBedrooms !== undefined) params.set('minBedrooms', String(filters.minBedrooms));
  params.set('sort', sort);
  params.set('page', String(page));
  params.set('pageSize', String(pageSize));

  return request<PagedResult<PropertySummary>>(`/properties?${params.toString()}`);
}

export function getProperty(id: string): Promise<PropertyDetail> {
  return request<PropertyDetail>(`/properties/${id}`);
}

export function uploadImages(propertyId: string, files: File[]): Promise<string[]> {
  const form = new FormData();
  for (const file of files) {
    form.append('files', file);
  }

  return request<string[]>(`/properties/${propertyId}/images`, { method: 'POST', body: form });
}

export function createListing(body: CreateListingRequest): Promise<string> {
  return request<string>('/properties', { method: 'POST', body: JSON.stringify(body) });
}

export function updateListing(id: string, body: Omit<CreateListingRequest, 'ownerId' | 'listingType' | 'propertyType'>): Promise<void> {
  return request<void>(`/properties/${id}`, { method: 'PUT', body: JSON.stringify(body) });
}

export function deleteListing(id: string): Promise<void> {
  return request<void>(`/properties/${id}`, { method: 'DELETE' });
}

export function markRented(id: string): Promise<void> {
  return request<void>(`/properties/${id}/mark-rented`, { method: 'PUT' });
}

export function markSold(id: string): Promise<void> {
  return request<void>(`/properties/${id}/mark-sold`, { method: 'PUT' });
}

export function withdrawListing(id: string): Promise<void> {
  return request<void>(`/properties/${id}/withdraw`, { method: 'PUT' });
}

export function relistProperty(id: string): Promise<void> {
  return request<void>(`/properties/${id}/relist`, { method: 'PUT' });
}
