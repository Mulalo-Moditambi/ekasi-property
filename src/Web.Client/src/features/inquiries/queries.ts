import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getInquiries, getMyLeads, submitInquiry } from './api';
import type { LeadsFilters, SubmitInquiryRequest } from './types';

export const inquiryKeys = {
  all: ['inquiries'] as const,
  forProperty: (propertyId: string) => [...inquiryKeys.all, 'property', propertyId] as const,
  leads: () => [...inquiryKeys.all, 'leads'] as const,
  leadList: (filters: LeadsFilters, page: number, pageSize: number) =>
    [...inquiryKeys.leads(), { filters, page, pageSize }] as const,
};

export function useInquiries(propertyId: string) {
  return useQuery({
    queryKey: inquiryKeys.forProperty(propertyId),
    queryFn: () => getInquiries(propertyId),
  });
}

export function useMyLeads(filters: LeadsFilters, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: inquiryKeys.leadList(filters, page, pageSize),
    queryFn: () => getMyLeads(filters, page, pageSize),
    placeholderData: keepPreviousData,
  });
}

export function useSubmitInquiry(propertyId: string) {
  const client = useQueryClient();

  return useMutation({
    mutationFn: (body: SubmitInquiryRequest) => submitInquiry(propertyId, body),
    onSuccess: () => {
      // The owner may be looking at this listing's inquiries in another tab.
      void client.invalidateQueries({ queryKey: inquiryKeys.forProperty(propertyId) });
      void client.invalidateQueries({ queryKey: inquiryKeys.leads() });
    },
  });
}
