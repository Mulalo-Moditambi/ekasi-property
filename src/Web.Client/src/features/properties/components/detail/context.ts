import { useOutletContext } from 'react-router-dom';
import type { PropertyDetail } from '../../types';

export interface PropertyDetailContext {
  property: PropertyDetail;
  isOwner: boolean;
}

/**
 * The tab routes render inside the detail page, which has already fetched and
 * narrowed the property. Passing it down through the outlet keeps each tab free
 * of loading and error states — by the time one renders, the data exists.
 */
export function usePropertyDetailContext(): PropertyDetailContext {
  return useOutletContext<PropertyDetailContext>();
}
