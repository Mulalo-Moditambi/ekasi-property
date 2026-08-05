import { useState } from 'react';
import { CarFront, DoorOpen, Droplets, Heart, Home, Images, Zap } from 'lucide-react';
import type { ComponentType } from 'react';
import { Badge } from '../../../shared/ui/badge';
import { cn } from '../../../shared/ui/utils';
import { useFavorite } from '../useFavorite';
import type { PropertySummary } from '../types';
import { ListingType, listingTypeLabels, propertyTypeLabels } from '../types';

const AMENITY_DISPLAY: {
  key: keyof Pick<PropertySummary, 'hasElectricity' | 'waterIncluded' | 'hasOwnEntrance' | 'hasParking'>;
  label: string;
  Icon: ComponentType<{ className?: string }>;
}[] = [
  { key: 'hasElectricity', label: 'Electricity', Icon: Zap },
  { key: 'waterIncluded', label: 'Water incl.', Icon: Droplets },
  { key: 'hasOwnEntrance', label: 'Own entrance', Icon: DoorOpen },
  { key: 'hasParking', label: 'Parking', Icon: CarFront },
];

export function AmenityList({ property, className }: { property: PropertySummary; className?: string }) {
  const present = AMENITY_DISPLAY.filter(({ key }) => property[key]);

  if (present.length === 0) {
    return null;
  }

  return (
    <ul className={cn('flex flex-wrap items-center gap-x-3 gap-y-1', className)}>
      {present.map(({ key, label, Icon }) => (
        <li key={key} className="flex items-center gap-1 text-2xs text-ink-2">
          <Icon className="size-3.5 text-ink-3" />
          {label}
        </li>
      ))}
    </ul>
  );
}

/** Sits over photography, so it is solid rather than tinted. */
export function ListingTypeBadge({ listingType }: { listingType: ListingType }) {
  return <Badge tone="solid">{listingTypeLabels[listingType]}</Badge>;
}

export function PropertyTypeBadge({ property }: { property: PropertySummary }) {
  return <Badge>{propertyTypeLabels[property.propertyType]}</Badge>;
}

export function Thumbnail({
  property,
  className,
}: {
  property: PropertySummary;
  className?: string;
}) {
  // A listing can reference an upload that is no longer on disk; fall back to the
  // placeholder rather than showing the browser's broken-image glyph.
  const [failed, setFailed] = useState(false);
  const cover = failed ? undefined : property.imageUrls[0];

  return (
    <div className={cn('relative overflow-hidden bg-surface-3', className)}>
      {/* Absolutely positioned so a portrait photo cannot stretch the row it sits in —
          the text column decides the height, the photo just fills whatever it gets. */}
      {cover ? (
        <img
          src={cover}
          alt=""
          loading="lazy"
          onError={() => setFailed(true)}
          className="absolute inset-0 size-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
        />
      ) : (
        <div className="absolute inset-0 flex items-center justify-center text-ink-3">
          <Home className="size-6" />
        </div>
      )}
      {property.imageUrls.length > 1 && (
        <span className="absolute bottom-1.5 left-1.5 flex items-center gap-1 rounded-sm bg-black/65 px-1.5 py-0.5 text-2xs font-medium text-white">
          <Images className="size-3" />
          {property.imageUrls.length}
        </span>
      )}
    </div>
  );
}

/**
 * Favourite toggle. Sits above the card's link overlay and swallows the click,
 * with an immediate press-scale so the toggle feels acknowledged before any
 * state round-trip.
 */
export function SaveButton({ id, className }: { id: string; className?: string }) {
  const { saved, toggle } = useFavorite(id);

  return (
    <button
      type="button"
      onClick={toggle}
      aria-pressed={saved}
      aria-label={saved ? 'Remove from saved' : 'Save this listing'}
      className={cn(
        'relative z-10 grid size-8 place-items-center rounded-full bg-white/90 shadow-sm',
        'text-ink-2 backdrop-blur outline-none',
        'transition-[transform,color,background-color] duration-200 ease-[cubic-bezier(0.4,0,0.2,1)]',
        'hover:scale-105 hover:bg-white hover:text-danger active:scale-90',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta',
        saved && 'text-danger',
        className,
      )}
    >
      <Heart className={cn('size-4', saved && 'fill-current')} />
    </button>
  );
}

/**
 * Beds / baths / type, separated by hairline dividers. Only fields the API
 * actually returns — floor area, garage count and agent are absent from the
 * domain model, so they are omitted rather than invented.
 */
export function SpecRow({ property, className }: { property: PropertySummary; className?: string }) {
  const specs = [
    { key: 'beds', label: property.bedrooms === 1 ? 'bed' : 'beds', value: property.bedrooms },
    { key: 'baths', label: property.bathrooms === 1 ? 'bath' : 'baths', value: property.bathrooms },
  ];

  return (
    <ul className={cn('flex items-center gap-3', className)}>
      {specs.map((spec) => (
        <li key={spec.key} className="flex items-center gap-3 text-sm text-ink-2">
          <span>
            <span className="tnum font-semibold text-ink">{spec.value}</span> {spec.label}
          </span>
          <span className="h-3.5 w-px bg-line" aria-hidden="true" />
        </li>
      ))}
      <li className="truncate text-sm text-ink-2">{propertyTypeLabels[property.propertyType]}</li>
    </ul>
  );
}
