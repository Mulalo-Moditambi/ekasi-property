import { request } from '../../shared/api/client';
import type { PagedResult } from '../properties/types';
import type { Inquiry, Lead, LeadsFilters, SubmitInquiryRequest } from './types';

export function submitInquiry(propertyId: string, body: SubmitInquiryRequest): Promise<string> {
  return request<string>(`/properties/${propertyId}/inquiries`, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function getInquiries(propertyId: string): Promise<Inquiry[]> {
  return request<Inquiry[]>(`/properties/${propertyId}/inquiries`);
}

export function getMyLeads(filters: LeadsFilters, page = 1, pageSize = 20): Promise<PagedResult<Lead>> {
  const params = new URLSearchParams();

  if (filters.propertyId) params.set('propertyId', filters.propertyId);
  if (filters.search) params.set('search', filters.search);
  params.set('page', String(page));
  params.set('pageSize', String(pageSize));

  return request<PagedResult<Lead>>(`/inquiries/mine?${params.toString()}`);
}
