export interface Inquiry {
  id: string;
  propertyId: string;
  name: string;
  email: string;
  phone: string | null;
  message: string;
  createdAt: string;
}

export interface SubmitInquiryRequest {
  name: string;
  email: string;
  phone?: string;
  message: string;
}

export interface Lead extends Inquiry {
  propertyTitle: string;
  propertyTownship: string;
}

export interface LeadsFilters {
  propertyId?: string;
  search?: string;
}
