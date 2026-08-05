import { Suspense, lazy, useState } from 'react';
import { Link, Outlet, useParams } from 'react-router-dom';
import { ChevronRight, Heart, MapPin, Share2 } from 'lucide-react';
import { paths } from '../app/routes/paths';
import { NotFoundPage } from '../app/routes/StatusPage';
import { useAuth } from '../features/auth/AuthContext';
import { DetailSkeleton } from '../features/properties/components/detail/DetailSkeleton';
import { DetailTabs } from '../features/properties/components/detail/DetailTabs';
import { OwnerPanel } from '../features/properties/components/detail/OwnerPanel';
import type { PropertyDetailContext } from '../features/properties/components/detail/context';
import { useProperty } from '../features/properties/queries';
import { useFavorite } from '../features/properties/useFavorite';
import { ApiError } from '../shared/api/client';
import { errorMessage } from '../shared/api/queryClient';
import { useToast } from '../shared/components/Toast';
import { priceParts, relativeTime } from '../shared/format';
import { Badge } from '../shared/ui/badge';
import { Button } from '../shared/ui/button';
import { Notice } from '../shared/ui/form';
import { Skeleton } from '../shared/ui/skeleton';
import { cn } from '../shared/ui/utils';
import { PropertyStatus, listingTypeLabels, propertyStatusLabels, propertyTypeLabels } from '../features/properties/types';

/**
 * The gallery pulls in the Radix dialog for its lightbox, which no other public
 * page needs. Splitting it out keeps that weight off the first paint of the
 * page a shared listing link lands on.
 */
const PropertyGallery = lazy(() =>
  import('../features/properties/components/PropertyGallery').then((module) => ({
    default: module.PropertyGallery,
  })),
);

/**
 * The contact form drags in react-hook-form and zod, and the inquiries panel is
 * owner-only. Neither belongs in the bundle a first-time visitor downloads to
 * read a listing, so both load on demand.
 */
const ContactOwnerForm = lazy(() =>
  import('../features/inquiries/components/ContactOwnerForm').then((module) => ({
    default: module.ContactOwnerForm,
  })),
);

const InquiriesPanel = lazy(() =>
  import('../features/inquiries/components/InquiriesPanel').then((module) => ({
    default: module.InquiriesPanel,
  })),
);

const STATUS_TONE = {
  [PropertyStatus.Listed]: 'cta',
  [PropertyStatus.Rented]: 'info',
  [PropertyStatus.Sold]: 'violet',
  [PropertyStatus.Withdrawn]: 'neutral',
} as const;

/**
 * Container for the listing: it owns the fetch and the page chrome, and hands a
 * guaranteed-present property to the tab routes through the outlet context.
 */
export function PropertyDetailPage() {
  const { propertyId } = useParams<{ propertyId: string }>();
  const { userId } = useAuth();
  const { notify } = useToast();
  const { data: property, isPending, error } = useProperty(propertyId);
  const { saved, toggle } = useFavorite(propertyId ?? '');
  const [shareBusy, setShareBusy] = useState(false);

  if (isPending) {
    return <DetailSkeleton />;
  }

  // A deleted or mistyped listing is a missing page, not a broken one.
  if (error instanceof ApiError && error.status === 404) {
    return <NotFoundPage />;
  }

  if (error || !property) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
        <Notice>{errorMessage(error, 'Could not load the listing')}</Notice>
      </div>
    );
  }

  const isOwner = userId !== null && userId === property.ownerId;
  const isListed = property.status === PropertyStatus.Listed;
  const { amount, period } = priceParts(property.price, property.listingType);
  const outletContext: PropertyDetailContext = { property, isOwner };

  const shareTitle = property.title;

  async function handleShare() {
    const url = window.location.href;
    setShareBusy(true);

    try {
      if (navigator.share) {
        await navigator.share({ title: shareTitle, url });
        return;
      }

      await navigator.clipboard.writeText(url);
      notify('Link copied to clipboard');
    } catch {
      // A cancelled share sheet and a blocked clipboard both land here; neither
      // is worth interrupting the user over.
    } finally {
      setShareBusy(false);
    }
  }

  return (
    // The extra bottom padding on mobile clears the sticky contact bar.
    <div className="mx-auto max-w-[1400px] px-4 pb-24 pt-5 sm:px-6 lg:pb-16">
      <nav aria-label="Breadcrumb" className="mb-4 flex items-center gap-1.5 text-sm text-ink-3">
        <Link
          to={paths.properties.root}
          className="rounded transition-colors hover:text-ink focus-visible:outline-2 focus-visible:outline-cta"
        >
          Browse
        </Link>
        <ChevronRight className="size-3.5" />
        <span>{property.township}</span>
        <ChevronRight className="size-3.5" />
        <span className="truncate text-ink-2">{property.title}</span>
      </nav>

      <Suspense fallback={<Skeleton className="aspect-[4/3] w-full rounded-2xl sm:aspect-auto sm:h-[26rem]" />}>
        <PropertyGallery urls={property.images.map((image) => image.url)} alt={property.title} />
      </Suspense>

      <div className="mt-6 lg:grid lg:grid-cols-[minmax(0,1fr)_22rem] lg:gap-8">
        <div className="min-w-0">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div className="min-w-0">
              <div className="mb-2 flex flex-wrap items-center gap-1.5">
                <Badge tone="solid">{listingTypeLabels[property.listingType]}</Badge>
                <Badge>{propertyTypeLabels[property.propertyType]}</Badge>
                <Badge tone={STATUS_TONE[property.status]}>
                  {propertyStatusLabels[property.status]}
                </Badge>
              </div>

              <h1 className="text-2xl font-bold tracking-tight text-ink sm:text-3xl">
                {property.title}
              </h1>

              <p className="mt-2 flex items-start gap-1.5 text-sm text-ink-2">
                <MapPin className="mt-0.5 size-4 shrink-0 text-ink-3" />
                {property.township}, {property.city}, {property.province}
              </p>
            </div>

            <div className="flex shrink-0 items-center gap-2">
              <Button
                variant="outline"
                size="icon"
                aria-pressed={saved}
                aria-label={saved ? 'Remove from saved listings' : 'Save this listing'}
                onClick={toggle}
                className={cn(saved && 'border-danger/30 text-danger')}
              >
                <Heart className={cn(saved && 'fill-current')} />
              </Button>
              <Button
                variant="outline"
                size="icon"
                aria-label="Share this listing"
                disabled={shareBusy}
                onClick={handleShare}
              >
                <Share2 />
              </Button>
            </div>
          </div>

          <div className="mt-5 flex flex-wrap items-baseline gap-2 border-y border-line py-4">
            <p className="tnum text-3xl font-bold tracking-tight text-ink">{amount}</p>
            {period && <span className="text-sm text-ink-3">{period}</span>}
            <span className="ml-auto text-xs text-ink-3">
              Listed {relativeTime(property.createdAt)}
            </span>
          </div>

          <DetailTabs propertyId={property.id} />
          <Outlet context={outletContext} />

          {isOwner && (
            <div className="mt-8">
              <Suspense fallback={<Skeleton className="h-48 rounded-2xl" />}>
                <InquiriesPanel propertyId={property.id} />
              </Suspense>
            </div>
          )}
        </div>

        {/* Sticky contact/manage panel, locked to the viewport on desktop. */}
        <aside id="contact-panel" className="mt-8 scroll-mt-20 lg:mt-0">
          <div className="lg:sticky lg:top-20">
            <div className="rounded-2xl border border-line bg-surface p-5 shadow-sm">
              {isOwner && <OwnerPanel property={property} />}
              {!isOwner && isListed && (
                <Suspense fallback={<Skeleton className="h-80 rounded-xl" />}>
                  <ContactOwnerForm propertyId={property.id} />
                </Suspense>
              )}
              {!isOwner && !isListed && (
                <p className="text-sm text-ink-2">
                  This property is currently{' '}
                  {propertyStatusLabels[property.status].toLowerCase()} and not taking inquiries.
                </p>
              )}
            </div>
          </div>
        </aside>
      </div>

      {/* Mobile: the contact action follows the reader down the page. */}
      {!isOwner && isListed && (
        <div className="fixed inset-x-0 bottom-0 z-40 border-t border-line bg-surface/95 p-3 backdrop-blur-md lg:hidden">
          <Button
            variant="cta"
            size="lg"
            className="w-full"
            onClick={() =>
              document.getElementById('contact-panel')?.scrollIntoView({ behavior: 'smooth' })
            }
          >
            Contact the owner
          </Button>
        </div>
      )}
    </div>
  );
}
