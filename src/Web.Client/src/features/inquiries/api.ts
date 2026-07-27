import { request } from '../../shared/api/client';
import type { Inquiry, SubmitInquiryRequest } from './types';

export function submitInquiry(propertyId: string, body: SubmitInquiryRequest): Promise<string> {
  return request<string>(`/properties/${propertyId}/inquiries`, {
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export function getInquiries(propertyId: string): Promise<Inquiry[]> {
  return request<Inquiry[]>(`/properties/${propertyId}/inquiries`);
}
