import { useState } from 'react';
import type { ChangeEvent, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createListing, uploadImages } from '../api/properties';
import { useAuth } from '../auth/AuthContext';
import { ListingType, PropertyType, propertyTypeLabels } from '../types/property';

const MAX_PHOTOS = 10;

export function CreateListingPage() {
  const { userId } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    title: '',
    description: '',
    listingType: ListingType.Rent,
    propertyType: PropertyType.Backroom,
    price: '',
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
  });
  const [photos, setPhotos] = useState<File[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function handlePhotosSelected(event: ChangeEvent<HTMLInputElement>) {
    const selected = Array.from(event.target.files ?? []);
    setPhotos((current) => [...current, ...selected].slice(0, MAX_PHOTOS));
    event.target.value = '';
  }

  function removePhoto(index: number) {
    setPhotos((current) => current.filter((_, i) => i !== index));
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!userId) return;

    setBusy(true);
    setError(null);

    let propertyId: string;
    try {
      propertyId = await createListing({
        ownerId: userId,
        ...form,
        price: Number(form.price),
      });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not create the listing');
      setBusy(false);
      return;
    }

    if (photos.length > 0) {
      try {
        await uploadImages(propertyId, photos);
      } catch {
        // The listing exists; photos can be added from the listing page.
      }
    }

    navigate(`/properties/${propertyId}`);
  }

  return (
    <div className="page narrow">
      <h1>List a property</h1>
      <p className="muted">
        Renting out a backroom or cottage, or selling a house? Put it in front of people looking ekasi.
      </p>

      <form className="stacked-form" onSubmit={handleSubmit}>
        <label>
          Listing type
          <select
            value={form.listingType}
            onChange={(e) => setForm({ ...form, listingType: Number(e.target.value) as ListingType })}
          >
            <option value={ListingType.Rent}>To Rent</option>
            <option value={ListingType.Sale}>For Sale</option>
          </select>
        </label>

        <label>
          Property type
          <select
            value={form.propertyType}
            onChange={(e) => setForm({ ...form, propertyType: Number(e.target.value) as PropertyType })}
          >
            {Object.entries(propertyTypeLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
        </label>

        <label>
          Title
          <input
            type="text"
            required
            maxLength={200}
            placeholder="e.g. Neat backroom with own entrance in Zola"
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
          />
        </label>

        <label>
          Description
          <textarea
            required
            maxLength={4000}
            rows={5}
            placeholder="Describe the place: size, electricity setup, water, entrance, rules…"
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
          />
        </label>

        <label>
          {form.listingType === ListingType.Rent ? 'Monthly rent (R)' : 'Asking price (R)'}
          <input
            type="number"
            required
            min="1"
            value={form.price}
            onChange={(e) => setForm({ ...form, price: e.target.value })}
          />
        </label>

        <fieldset>
          <legend>Address</legend>
          <label>
            Street
            <input
              type="text"
              required
              value={form.street}
              onChange={(e) => setForm({ ...form, street: e.target.value })}
            />
          </label>
          <label>
            Township / Section
            <input
              type="text"
              required
              placeholder="e.g. Orlando West"
              value={form.township}
              onChange={(e) => setForm({ ...form, township: e.target.value })}
            />
          </label>
          <label>
            City
            <input
              type="text"
              required
              placeholder="e.g. Soweto"
              value={form.city}
              onChange={(e) => setForm({ ...form, city: e.target.value })}
            />
          </label>
          <label>
            Province
            <input
              type="text"
              required
              placeholder="e.g. Gauteng"
              value={form.province}
              onChange={(e) => setForm({ ...form, province: e.target.value })}
            />
          </label>
          <label>
            Postal code
            <input
              type="text"
              required
              maxLength={10}
              value={form.postalCode}
              onChange={(e) => setForm({ ...form, postalCode: e.target.value })}
            />
          </label>
        </fieldset>

        <div className="form-row">
          <label>
            Bedrooms
            <input
              type="number"
              min="0"
              value={form.bedrooms}
              onChange={(e) => setForm({ ...form, bedrooms: Number(e.target.value) })}
            />
          </label>
          <label>
            Bathrooms
            <input
              type="number"
              min="0"
              value={form.bathrooms}
              onChange={(e) => setForm({ ...form, bathrooms: Number(e.target.value) })}
            />
          </label>
        </div>

        <fieldset>
          <legend>Photos</legend>
          <label>
            Add up to {MAX_PHOTOS} photos (JPEG, PNG, or WebP — max 5&nbsp;MB each)
            <input
              type="file"
              multiple
              accept="image/jpeg,image/png,image/webp"
              onChange={handlePhotosSelected}
            />
          </label>
          {photos.length > 0 && (
            <div className="photo-previews">
              {photos.map((photo, index) => (
                <div key={`${photo.name}-${index}`} className="photo-preview">
                  <img src={URL.createObjectURL(photo)} alt={photo.name} />
                  <button type="button" aria-label={`Remove ${photo.name}`} onClick={() => removePhoto(index)}>
                    ✕
                  </button>
                </div>
              ))}
            </div>
          )}
        </fieldset>

        <fieldset>
          <legend>Amenities</legend>
          <label className="checkbox">
            <input
              type="checkbox"
              checked={form.hasElectricity}
              onChange={(e) => setForm({ ...form, hasElectricity: e.target.checked })}
            />
            Electricity (own or prepaid meter)
          </label>
          <label className="checkbox">
            <input
              type="checkbox"
              checked={form.waterIncluded}
              onChange={(e) => setForm({ ...form, waterIncluded: e.target.checked })}
            />
            Water included
          </label>
          <label className="checkbox">
            <input
              type="checkbox"
              checked={form.hasOwnEntrance}
              onChange={(e) => setForm({ ...form, hasOwnEntrance: e.target.checked })}
            />
            Own entrance
          </label>
          <label className="checkbox">
            <input
              type="checkbox"
              checked={form.hasParking}
              onChange={(e) => setForm({ ...form, hasParking: e.target.checked })}
            />
            Parking
          </label>
        </fieldset>

        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={busy}>
          {busy ? 'Publishing…' : 'Publish listing'}
        </button>
      </form>
    </div>
  );
}
