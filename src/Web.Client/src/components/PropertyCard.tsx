import { Link } from 'react-router-dom';
import type { PropertySummary } from '../types/property';
import { formatPrice, listingTypeLabels, propertyTypeLabels } from '../types/property';
import { ImageCarousel } from './ImageCarousel';

export function PropertyCard({ property }: { property: PropertySummary }) {
  return (
    <Link to={`/properties/${property.id}`} className="property-card">
      <ImageCarousel urls={property.imageUrls} alt={property.title} />
      <div className="card-body">
        <div className="card-badges">
          <span className="badge badge-listing">{listingTypeLabels[property.listingType]}</span>
          <span className="badge badge-type">{propertyTypeLabels[property.propertyType]}</span>
        </div>
        <h3>{property.title}</h3>
        <p className="price">{formatPrice(property.price, property.listingType)}</p>
        <p className="location">
          {property.township}, {property.city}, {property.province}
        </p>
        <ul className="features">
          <li>{property.bedrooms} bed</li>
          <li>{property.bathrooms} bath</li>
          {property.hasElectricity && <li>Electricity</li>}
          {property.waterIncluded && <li>Water incl.</li>}
          {property.hasOwnEntrance && <li>Own entrance</li>}
          {property.hasParking && <li>Parking</li>}
        </ul>
      </div>
    </Link>
  );
}
