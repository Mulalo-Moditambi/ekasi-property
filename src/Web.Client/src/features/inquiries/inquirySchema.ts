import { z } from 'zod';
import type { SubmitInquiryRequest } from './types';

export const inquirySchema = z.object({
  name: z.string().trim().min(2, 'Tell the owner who you are').max(100),
  email: z.string().trim().email('Enter a valid email address').max(255),
  // Optional, but if given it should look like a phone number — an owner who
  // can't call you back is worse than no number at all.
  phone: z
    .string()
    .trim()
    .max(20)
    .regex(/^[\d+\s()-]*$/, 'Use digits, spaces and + ( ) - only')
    .optional(),
  message: z
    .string()
    .trim()
    .min(10, 'Add a little more detail — owners reply to specific questions')
    .max(2000),
});

export type InquiryFormValues = z.infer<typeof inquirySchema>;

export const emptyInquiry: InquiryFormValues = { name: '', email: '', phone: '', message: '' };

export function toInquiryRequest(values: InquiryFormValues): SubmitInquiryRequest {
  return { ...values, phone: values.phone?.trim() || undefined };
}
