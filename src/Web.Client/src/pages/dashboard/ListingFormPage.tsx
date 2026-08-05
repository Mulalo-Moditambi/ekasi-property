import { useEffect, useMemo } from 'react';
import { FormProvider, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useNavigate, useParams } from 'react-router-dom';
import { paths } from '../../app/routes/paths';
import { useAuth } from '../../features/auth/AuthContext';
import { AddressStep } from '../../features/properties/components/listing-form/AddressStep';
import { AmenitiesStep } from '../../features/properties/components/listing-form/AmenitiesStep';
import { BasicsStep } from '../../features/properties/components/listing-form/BasicsStep';
import { MAX_PHOTOS, PhotosStep } from '../../features/properties/components/listing-form/PhotosStep';
import { RoomsStep } from '../../features/properties/components/listing-form/RoomsStep';
import { PropertyCard } from '../../features/properties/components/PropertyCard';
import {
  emptyListing,
  listingSchema,
  listingToFormValues,
  toCreateRequest,
  toUpdateRequest,
} from '../../features/properties/listingSchema';
import type { ListingFormValues, ListingValues } from '../../features/properties/listingSchema';
import {
  useCreateListing,
  useDeleteImage,
  useProperty,
  useReorderImages,
  useUpdateListing,
  useUploadImages,
} from '../../features/properties/queries';
import { usePhotoDrafts } from '../../features/properties/usePhotoDrafts';
import type { PropertyImage } from '../../features/properties/types';
import { ListingType, PropertyType } from '../../features/properties/types';
import { errorMessage } from '../../shared/api/queryClient';
import { useToast } from '../../shared/components/Toast';
import { Button } from '../../shared/ui/button';
import { Notice } from '../../shared/ui/form';
import { Skeleton } from '../../shared/ui/skeleton';

/**
 * Create and edit share this page because they share a contract — the only
 * differences are which fields are immutable and whether photos can be managed
 * before the listing exists.
 *
 * It is a container: validation lives in `listingSchema`, the network calls in
 * `queries`, the pending-photo lifecycle in `usePhotoDrafts`, and each block of
 * fields in its own step component.
 */
export function ListingFormPage() {
  const { propertyId } = useParams<{ propertyId: string }>();
  const isEdit = Boolean(propertyId);
  const { userId } = useAuth();
  const navigate = useNavigate();
  const { notify } = useToast();

  const { data: property, isPending: loadingProperty, error: loadError } = useProperty(propertyId);

  const createListing = useCreateListing();
  const updateListing = useUpdateListing();
  const uploadImages = useUploadImages();
  const deleteImage = useDeleteImage();
  const reorderImages = useReorderImages();

  const existingImages = useMemo<PropertyImage[]>(() => property?.images ?? [], [property]);
  const photos = usePhotoDrafts(MAX_PHOTOS, existingImages.length);

  const form = useForm<ListingFormValues>({
    resolver: zodResolver(listingSchema),
    defaultValues: emptyListing,
    // Errors appear once a field has been visited, then update live — nagging
    // about a field nobody has filled in yet is what makes forms feel hostile.
    mode: 'onTouched',
  });

  const { reset, handleSubmit, watch } = form;

  useEffect(() => {
    if (property) {
      reset(listingToFormValues(property));
    }
  }, [property, reset]);

  const busy =
    createListing.isPending || updateListing.isPending || uploadImages.isPending || form.formState.isSubmitting;

  async function onValid(values: ListingFormValues) {
    if (!userId) {
      return;
    }

    // The resolver has already parsed these; this restates the output type.
    const parsed = values as unknown as ListingValues;

    try {
      let targetId = propertyId;

      if (isEdit && propertyId) {
        await updateListing.mutateAsync({ id: propertyId, body: toUpdateRequest(parsed) });
      } else {
        targetId = await createListing.mutateAsync(toCreateRequest(parsed, userId));
      }

      if (targetId && photos.files.length > 0) {
        try {
          await uploadImages.mutateAsync({ id: targetId, files: photos.files });
          photos.clear();
        } catch {
          // The listing itself saved — a photo failure shouldn't read as a lost draft.
          notify('Listing saved, but some photos failed to upload', 'error');
        }
      }

      notify(isEdit ? 'Listing updated' : 'Listing published');
      navigate(isEdit ? paths.manager.properties : paths.properties.detail(targetId as string));
    } catch (error) {
      // A server-side rule the client schema doesn't model (or a network fault).
      form.setError('root', { message: errorMessage(error, 'Could not save the listing') });
    }
  }

  function handleReorder(from: number, to: number) {
    if (!propertyId || to < 0 || to >= existingImages.length) {
      return;
    }

    const next = [...existingImages];
    const [moved] = next.splice(from, 1);
    next.splice(to, 0, moved);

    reorderImages.mutate(
      { id: propertyId, imageIds: next.map((image) => image.id) },
      { onError: (error) => notify(errorMessage(error, 'Could not save the new photo order'), 'error') },
    );
  }

  function handleRemoveExisting(image: PropertyImage) {
    if (!propertyId) {
      return;
    }

    deleteImage.mutate(
      { id: propertyId, imageId: image.id },
      { onError: (error) => notify(errorMessage(error, 'Could not remove the photo'), 'error') },
    );
  }

  if (isEdit && loadingProperty) {
    return (
      <div className="space-y-4" aria-hidden="true">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-96 rounded-2xl" />
      </div>
    );
  }

  if (isEdit && loadError) {
    return <Notice>{errorMessage(loadError, 'Could not load the listing')}</Notice>;
  }

  const values = watch();

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight text-ink">
          {isEdit ? 'Edit listing' : 'List a property'}
        </h1>
        <p className="mt-1 text-sm text-ink-2">
          {isEdit
            ? 'Update the details, photos and amenities.'
            : 'Free to list — you deal with tenants directly.'}
        </p>
      </div>

      <div className="lg:grid lg:grid-cols-[minmax(0,1fr)_20rem] lg:gap-8">
        <FormProvider {...form}>
          <form className="space-y-6" noValidate onSubmit={handleSubmit(onValid)}>
            <BasicsStep isEdit={isEdit} />
            <AddressStep />
            <RoomsStep />

            {/*
              Photos are only manageable once the listing has an id — the upload
              endpoint is scoped to a property. Before that they queue as drafts
              and upload immediately after the listing is created.
            */}
            <PhotosStep
              existing={existingImages}
              drafts={photos.drafts}
              onSelectFiles={photos.add}
              onRemoveDraft={photos.remove}
              onRemoveExisting={handleRemoveExisting}
              onReorder={handleReorder}
            />

            <AmenitiesStep />

            {form.formState.errors.root && <Notice>{form.formState.errors.root.message}</Notice>}

            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={() => navigate(-1)}>
                Cancel
              </Button>
              <Button type="submit" variant="cta" size="lg" disabled={busy}>
                {busy ? 'Saving…' : isEdit ? 'Save changes' : 'Publish listing'}
              </Button>
            </div>
          </form>
        </FormProvider>

        {/* Live preview of exactly the card that will appear in results. */}
        <aside className="mt-8 lg:mt-0">
          <div className="lg:sticky lg:top-6">
            <p className="mb-2 text-2xs font-semibold uppercase tracking-wider text-ink-3">
              Live preview
            </p>
            <div className="pointer-events-none">
              <PropertyCard
                property={{
                  id: propertyId ?? 'preview',
                  title: values.title || 'Your listing title',
                  listingType: values.listingType as ListingType,
                  propertyType: values.propertyType as PropertyType,
                  price: Number(values.price) || 0,
                  township: values.township || 'Township',
                  city: values.city || 'City',
                  province: values.province || 'Province',
                  bedrooms: Number(values.bedrooms) || 0,
                  bathrooms: Number(values.bathrooms) || 0,
                  hasElectricity: values.hasElectricity,
                  waterIncluded: values.waterIncluded,
                  hasOwnEntrance: values.hasOwnEntrance,
                  hasParking: values.hasParking,
                  createdAt: property?.createdAt ?? new Date().toISOString(),
                  imageUrls:
                    existingImages.length > 0
                      ? existingImages.map((image) => image.url)
                      : photos.drafts.map((draft) => draft.previewUrl),
                }}
              />
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
