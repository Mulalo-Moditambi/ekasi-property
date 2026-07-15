// Mirrors the backend Domain.Properties enums (serialized as numbers).
export enum ListingType {
  Rent = 0,
  Sale = 1,
}

export enum PropertyType {
  Backroom = 0,
  Cottage = 1,
  Room = 2,
  House = 3,
  Flat = 4,
}

export enum PropertyStatus {
  Listed = 0,
  Rented = 1,
  Sold = 2,
  Withdrawn = 3,
}

export const listingTypeLabels: Record<ListingType, string> = {
  [ListingType.Rent]: 'To Rent',
  [ListingType.Sale]: 'For Sale',
};

export const propertyTypeLabels: Record<PropertyType, string> = {
  [PropertyType.Backroom]: 'Backroom',
  [PropertyType.Cottage]: 'Cottage',
  [PropertyType.Room]: 'Room',
  [PropertyType.House]: 'House',
  [PropertyType.Flat]: 'Flat',
};

export const propertyStatusLabels: Record<PropertyStatus, string> = {
  [PropertyStatus.Listed]: 'Listed',
  [PropertyStatus.Rented]: 'Rented Out',
  [PropertyStatus.Sold]: 'Sold',
  [PropertyStatus.Withdrawn]: 'Withdrawn',
};

export interface PropertyImage {
  id: string;
  url: string;
}

export interface PropertySummary {
  id: string;
  title: string;
  listingType: ListingType;
  propertyType: PropertyType;
  price: number;
  township: string;
  city: string;
  province: string;
  bedrooms: number;
  bathrooms: number;
  hasElectricity: boolean;
  waterIncluded: boolean;
  hasOwnEntrance: boolean;
  hasParking: boolean;
  createdAt: string;
  imageUrls: string[];
}

export interface PropertyDetail extends Omit<PropertySummary, 'imageUrls'> {
  ownerId: string;
  description: string;
  street: string;
  postalCode: string;
  status: PropertyStatus;
  updatedAt: string | null;
  images: PropertyImage[];
}

export interface SearchFilters {
  township?: string;
  listingType?: ListingType;
  propertyType?: PropertyType;
  minPrice?: number;
  maxPrice?: number;
  minBedrooms?: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}

export interface CreateListingRequest {
  ownerId: string;
  title: string;
  description: string;
  listingType: ListingType;
  propertyType: PropertyType;
  price: number;
  street: string;
  township: string;
  city: string;
  province: string;
  postalCode: string;
  bedrooms: number;
  bathrooms: number;
  hasElectricity: boolean;
  waterIncluded: boolean;
  hasOwnEntrance: boolean;
  hasParking: boolean;
}

export function formatPrice(price: number, listingType: ListingType): string {
  const amount = new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR',
    maximumFractionDigits: 0,
  }).format(price);

  return listingType === ListingType.Rent ? `${amount} / month` : amount;
}
