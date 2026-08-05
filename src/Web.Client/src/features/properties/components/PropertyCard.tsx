import { memo } from 'react';
import { Link } from 'react-router-dom';
import { MapPin } from 'lucide-react';
import { paths } from '../../../app/routes/paths';
import { isRecent, priceParts } from '../../../shared/format';
import { Badge } from '../../../shared/ui/badge';
import { cn } from '../../../shared/ui/utils';
import type { PropertySummary } from '../types';
import { usePrefetchOnIntent } from '../usePrefetchOnIntent';
import { AmenityList, ListingTypeBadge, SaveButton, SpecRow, Thumbnail } from './ListingParts';

/**
 * The primary results card: 4:3 media, status tag top-left, favourite top-right,
 * then price-led body copy. The whole card is a link; the favourite sits above
 * that overlay so it stays independently clickable.
 *
 * Memoised because the results grid re-renders on every filter keystroke while
 * the individual listings are usually the same objects from the query cache.
 */
export const PropertyCard = memo(function PropertyCard({
  property,
  className,
}: {
  property: PropertySummary;
  className?: string;
}) {
  const { amount, period } = priceParts(property.price, property.listingType);
  const prefetch = usePrefetchOnIntent(property.id);

  return (
    <article
      {...prefetch}
      className={cn(
        'group relative flex flex-col overflow-hidden rounded-2xl border border-line bg-surface',
        'shadow-sm transition-shadow duration-300 ease-[cubic-bezier(0.4,0,0.2,1)] hover:shadow-md',
        className,
      )}
    >
      <Link
        to={paths.properties.detail(property.id)}
        className="absolute inset-0 z-0 rounded-2xl outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta"
      >
        <span className="sr-only">{property.title}</span>
      </Link>

      <div className="relative aspect-[4/3] w-full overflow-hidden">
        <Thumbnail
          property={property}
          className="size-full [&_img]:transition-transform [&_img]:duration-300 [&_img]:ease-out [&_img]:group-hover:scale-105"
        />

        <div className="pointer-events-none absolute left-3 top-3 flex flex-wrap gap-1.5">
          <ListingTypeBadge listingType={property.listingType} />
          {isRecent(property.createdAt) && <Badge tone="cta">New</Badge>}
        </div>

        <SaveButton id={property.id} className="absolute right-3 top-3" />
      </div>

      <div className="flex flex-1 flex-col p-4">
        <div className="flex items-baseline gap-1.5">
          <p className="tnum text-2xl font-bold tracking-tight text-ink">{amount}</p>
          {period && <span className="text-sm text-ink-3">{period}</span>}
        </div>

        <h3 className="mt-1.5 truncate text-[15px] font-semibold text-ink">{property.title}</h3>

        <p className="mt-1 flex items-center gap-1 truncate text-sm text-ink-3">
          <MapPin className="size-3.5 shrink-0" />
          {property.township}, {property.city}
        </p>

        <SpecRow property={property} className="mt-3 border-t border-line pt-3" />

        <AmenityList property={property} className="mt-2.5" />
      </div>
    </article>
  );
});
