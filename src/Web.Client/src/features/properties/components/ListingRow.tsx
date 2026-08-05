import { memo } from 'react';
import { Link } from 'react-router-dom';
import { Bath, BedDouble, MapPin } from 'lucide-react';
import { paths } from '../../../app/routes/paths';
import { priceParts, isRecent, relativeTime } from '../../../shared/format';
import { Badge } from '../../../shared/ui/badge';
import type { PropertySummary } from '../types';
import { usePrefetchOnIntent } from '../usePrefetchOnIntent';
import { AmenityList, ListingTypeBadge, PropertyTypeBadge, SaveButton, Thumbnail } from './ListingParts';

/**
 * The default results row: a wide, scannable record. Everything a renter filters
 * on sits in a fixed position across every row — photo, then identity, then the
 * specs column, with price right-aligned so a column of prices reads as a column.
 *
 * From `sm` up the row is roughly twice as tall, giving the photography room to
 * carry the listing and letting the copy breathe. Below `sm` it stays compact —
 * on a phone the smaller row is what keeps the results list scrollable.
 */
export const ListingRow = memo(function ListingRow({ property }: { property: PropertySummary }) {
  const { amount, period } = priceParts(property.price, property.listingType);
  const prefetch = usePrefetchOnIntent(property.id);

  return (
    <article
      {...prefetch}
      className="group relative flex min-h-[132px] overflow-hidden rounded-2xl border border-line bg-surface shadow-sm transition-shadow duration-300 ease-[cubic-bezier(0.4,0,0.2,1)] hover:shadow-md sm:min-h-[264px]"
    >
      <Link
        to={paths.properties.detail(property.id)}
        className="absolute inset-0 rounded-2xl outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta"
      >
        <span className="sr-only">{property.title}</span>
      </Link>

      {/* No fixed height — the flex row stretches it, so the photo always fills its column. */}
      <Thumbnail property={property} className="w-[132px] shrink-0 sm:w-[272px] lg:w-[336px]" />

      <div className="flex min-w-0 flex-1 flex-col justify-between gap-2 p-3 sm:flex-row sm:gap-6 sm:p-5">
        <div className="flex min-w-0 flex-1 flex-col sm:justify-between">
          <div className="min-w-0">
            <div className="mb-1 flex flex-wrap items-center gap-1.5 sm:mb-2">
              <ListingTypeBadge listingType={property.listingType} />
              <PropertyTypeBadge property={property} />
              {isRecent(property.createdAt) && <Badge tone="cta">New</Badge>}
            </div>

            <h3 className="truncate text-[15px] font-semibold leading-snug text-ink sm:text-xl sm:tracking-tight">
              {property.title}
            </h3>

            <p className="mt-0.5 flex items-center gap-1 truncate text-xs text-ink-2 sm:mt-1.5 sm:text-sm">
              <MapPin className="size-3.5 shrink-0 text-ink-3 sm:size-4" />
              {property.township}, {property.city}, {property.province}
            </p>
          </div>

          <div className="mt-2 sm:mt-0">
            <div className="flex items-center gap-3 text-xs font-medium text-ink sm:gap-4 sm:text-sm">
              <span className="flex items-center gap-1 sm:gap-1.5">
                <BedDouble className="size-3.5 text-ink-3 sm:size-4" />
                <span className="tnum">{property.bedrooms}</span> bed
              </span>
              <span className="h-3 w-px bg-line sm:h-4" aria-hidden="true" />
              <span className="flex items-center gap-1 sm:gap-1.5">
                <Bath className="size-3.5 text-ink-3 sm:size-4" />
                <span className="tnum">{property.bathrooms}</span> bath
              </span>
            </div>

            <AmenityList property={property} className="mt-2 sm:mt-3" />
          </div>
        </div>

        <div className="flex shrink-0 items-end justify-between gap-3 sm:w-40 sm:flex-col sm:items-end sm:justify-between">
          <div className="text-right">
            <p className="tnum text-lg font-semibold leading-tight tracking-tight text-ink sm:text-3xl sm:font-bold">
              {amount}
            </p>
            {period && <p className="text-2xs text-ink-3 sm:mt-0.5 sm:text-sm">{period}</p>}
          </div>
          <div className="flex items-center gap-2 sm:gap-3">
            <span className="text-2xs text-ink-3 sm:text-xs">{relativeTime(property.createdAt)}</span>
            <SaveButton id={property.id} />
          </div>
        </div>
      </div>
    </article>
  );
});
