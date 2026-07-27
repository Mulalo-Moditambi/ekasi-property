import { useCallback, useEffect, useRef, useState } from 'react';
import type { ChangeEvent } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  deleteListing,
  getProperty,
  markRented,
  markSold,
  relistProperty,
  uploadImages,
  withdrawListing,
} from '../features/properties/api';
import { useAuth } from '../features/auth/AuthContext';
import { Breadcrumbs } from '../shared/components/Breadcrumbs';
import { ContactOwnerForm } from '../features/inquiries/components/ContactOwnerForm';
import { InquiriesPanel } from '../features/inquiries/components/InquiriesPanel';
import { PropertyGallery } from '../features/properties/components/PropertyGallery';
import type { PropertyDetail } from '../features/properties/types';
import {
  ListingType,
  PropertyStatus,
  formatPrice,
  listingTypeLabels,
  propertyStatusLabels,
  propertyTypeLabels,
} from '../features/properties/types';

export function PropertyDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { userId } = useAuth();

  const [property, setProperty] = useState<PropertyDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const photoInputRef = useRef<HTMLInputElement>(null);

  const load = useCallback(async () => {
    if (!id) return;

    try {
      setProperty(await getProperty(id));
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not load the listing');
    }
  }, [id]);

  useEffect(() => {
    void load();
  }, [load]);

  async function runAction(action: (propertyId: string) => Promise<void>, refresh = true) {
    if (!id) return;
    setBusy(true);
    setError(null);

    try {
      await action(id);
      if (refresh) {
        await load();
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Action failed');
    } finally {
      setBusy(false);
    }
  }

  async function handlePhotosSelected(event: ChangeEvent<HTMLInputElement>) {
    if (!id) return;
    const files = Array.from(event.target.files ?? []);
    event.target.value = '';
    if (files.length === 0) return;

    setBusy(true);
    setError(null);

    try {
      await uploadImages(id, files);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not upload the photos');
    } finally {
      setBusy(false);
    }
  }

  async function handleDelete() {
    if (!window.confirm('Delete this listing permanently?')) return;

    await runAction(deleteListing, false);
    navigate('/');
  }

  if (error && !property) {
    return (
      <div className="page">
        <p className="error">{error}</p>
      </div>
    );
  }

  if (!property) {
    return (
      <div className="page">
        <p className="muted">Loading listing…</p>
      </div>
    );
  }

  const isOwner = userId !== null && userId === property.ownerId;
  const isListed = property.status === PropertyStatus.Listed;

  return (
    <div className="page detail">
      <Breadcrumbs
        items={[
          { label: 'Home', to: '/' },
          { label: property.township },
          { label: property.title },
        ]}
      />

      <header className="detail-header">
        <h1>{property.title}</h1>
        <p className="location">
          {property.street}, {property.township}, {property.city}, {property.province}, {property.postalCode}
        </p>
        <p className="price">{formatPrice(property.price, property.listingType)}</p>
      </header>

      <div className="detail-layout">
        <div className="detail-main">
          <PropertyGallery urls={property.images.map((image) => image.url)} alt={property.title} />

          <div className="card-badges detail-tags">
            <span className="badge badge-listing">{listingTypeLabels[property.listingType]}</span>
            <span className="badge badge-type">{propertyTypeLabels[property.propertyType]}</span>
            <span className={`badge status-${property.status}`}>{propertyStatusLabels[property.status]}</span>
          </div>

          <ul className="features">
            <li>{property.bedrooms} bedroom{property.bedrooms === 1 ? '' : 's'}</li>
            <li>{property.bathrooms} bathroom{property.bathrooms === 1 ? '' : 's'}</li>
            {property.hasElectricity && <li>Electricity</li>}
            {property.waterIncluded && <li>Water included</li>}
            {property.hasOwnEntrance && <li>Own entrance</li>}
            {property.hasParking && <li>Parking</li>}
          </ul>

          <section className="description">
            <h2>About this property</h2>
            <p>{property.description}</p>
          </section>

          {isOwner && <InquiriesPanel propertyId={property.id} />}
        </div>

        <aside className="detail-side">
          <div className="side-card">
            {!isOwner && isListed && <ContactOwnerForm propertyId={property.id} />}

            {!isOwner && !isListed && (
              <p className="muted">
                This property is currently {propertyStatusLabels[property.status].toLowerCase()} and not taking
                inquiries.
              </p>
            )}

            {isOwner && (
              <section className="owner-actions">
                <h2>Manage your listing</h2>
                {error && <p className="error">{error}</p>}
                <div className="actions">
                  {isListed && property.listingType === ListingType.Rent && (
                    <button type="button" disabled={busy} onClick={() => runAction(markRented)}>
                      Mark as rented
                    </button>
                  )}
                  {isListed && property.listingType === ListingType.Sale && (
                    <button type="button" disabled={busy} onClick={() => runAction(markSold)}>
                      Mark as sold
                    </button>
                  )}
                  {isListed && (
                    <button type="button" disabled={busy} onClick={() => runAction(withdrawListing)}>
                      Withdraw
                    </button>
                  )}
                  {(property.status === PropertyStatus.Rented || property.status === PropertyStatus.Withdrawn) && (
                    <button type="button" disabled={busy} onClick={() => runAction(relistProperty)}>
                      Relist
                    </button>
                  )}
                  <button
                    type="button"
                    disabled={busy || property.images.length >= 10}
                    onClick={() => photoInputRef.current?.click()}
                  >
                    Add photos ({property.images.length}/10)
                  </button>
                  <input
                    ref={photoInputRef}
                    type="file"
                    multiple
                    accept="image/jpeg,image/png,image/webp"
                    hidden
                    onChange={handlePhotosSelected}
                  />
                  <button type="button" className="danger" disabled={busy} onClick={handleDelete}>
                    Delete listing
                  </button>
                </div>
              </section>
            )}
          </div>
        </aside>
      </div>
    </div>
  );
}
