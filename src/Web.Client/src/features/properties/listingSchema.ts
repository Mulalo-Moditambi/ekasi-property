import { z } from 'zod';
import { ListingType, PropertyType } from './types';
import type { CreateListingRequest, PropertyDetail } from './types';

/**
 * The listing contract, stated once. The form renders from it, validation runs
 * off it, and the request body is derived from it — so a field can't be
 * validated one way and submitted another.
 *
 * Limits mirror the API's FluentValidation rules; where they disagree the
 * server still wins, and its message surfaces on the form.
 */
export const listingSchema = z.object({
  listingType: z.nativeEnum(ListingType),
  propertyType: z.nativeEnum(PropertyType),

  title: z
    .string()
    .trim()
    .min(10, 'Give the listing a title of at least 10 characters')
    .max(200, 'Keep the title under 200 characters'),

  description: z
    .string()
    .trim()
    .min(30, 'Describe the place in at least 30 characters — it is what gets people to enquire')
    .max(4000, 'Keep the description under 4000 characters'),

  // The input is `type="number"`, which yields a string; coerce here rather than
  // scattering `Number(...)` through the component.
  price: z.coerce
    .number({ message: 'Enter a price' })
    .positive('Price must be more than R0')
    .max(100_000_000, 'That price looks like a typo'),

  street: z.string().trim().min(1, 'Street is required').max(200),
  township: z.string().trim().min(1, 'Township or section is required').max(100),
  city: z.string().trim().min(1, 'City is required').max(100),
  province: z.string().trim().min(1, 'Province is required').max(100),
  postalCode: z
    .string()
    .trim()
    .regex(/^\d{4}$/, 'South African postal codes are 4 digits'),

  bedrooms: z.coerce.number().int('Whole numbers only').min(0).max(50),
  bathrooms: z.coerce.number().int('Whole numbers only').min(0).max(50),

  hasElectricity: z.boolean(),
  waterIncluded: z.boolean(),
  hasOwnEntrance: z.boolean(),
  hasParking: z.boolean(),
});

/** What the form holds. `price` is a string in the input, a number after parsing. */
export type ListingFormValues = z.input<typeof listingSchema>;
export type ListingValues = z.output<typeof listingSchema>;

export const emptyListing: ListingFormValues = {
  listingType: ListingType.Rent,
  propertyType: PropertyType.Backroom,
  title: '',
  description: '',
  price: '' as unknown as number,
  street: '',
  township: '',
  city: '',
  province: '',
  postalCode: '',
  bedrooms: 1,
  bathrooms: 1,
  hasElectricity: false,
  waterIncluded: false,
  hasOwnEntrance: false,
  hasParking: false,
};

export function listingToFormValues(property: PropertyDetail): ListingFormValues {
  return {
    listingType: property.listingType,
    propertyType: property.propertyType,
    title: property.title,
    description: property.description,
    price: property.price,
    street: property.street,
    township: property.township,
    city: property.city,
    province: property.province,
    postalCode: property.postalCode,
    bedrooms: property.bedrooms,
    bathrooms: property.bathrooms,
    hasElectricity: property.hasElectricity,
    waterIncluded: property.waterIncluded,
    hasOwnEntrance: property.hasOwnEntrance,
    hasParking: property.hasParking,
  };
}

export function toCreateRequest(values: ListingValues, ownerId: string): CreateListingRequest {
  return { ownerId, ...values };
}

/** Listing and property type are immutable after publishing, so edits omit them. */
export function toUpdateRequest(
  values: ListingValues,
): Omit<CreateListingRequest, 'ownerId' | 'listingType' | 'propertyType'> {
  const { listingType: _listingType, propertyType: _propertyType, ...rest } = values;
  return rest;
}
