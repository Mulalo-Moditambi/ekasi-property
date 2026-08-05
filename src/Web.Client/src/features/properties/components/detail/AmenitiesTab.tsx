import { CarFront, Check, DoorOpen, Droplets, Minus, Zap } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { cn } from '../../../../shared/ui/utils';
import { usePropertyDetailContext } from './context';
import type { PropertyDetail } from '../../types';

const AMENITIES: { key: keyof PropertyDetail; label: string; Icon: LucideIcon }[] = [
  { key: 'hasElectricity', label: 'Electricity', Icon: Zap },
  { key: 'waterIncluded', label: 'Water included', Icon: Droplets },
  { key: 'hasOwnEntrance', label: 'Own entrance', Icon: DoorOpen },
  { key: 'hasParking', label: 'Parking', Icon: CarFront },
];

/**
 * Shows what the place *doesn't* have as well. On the old page absent amenities
 * were filtered out, which left "no parking" and "we didn't say" looking
 * identical — the single most common question a seeker asks.
 */
export function AmenitiesTab() {
  const { property } = usePropertyDetailContext();

  return (
    <div className="pt-6">
      <h2 className="text-lg font-semibold tracking-tight text-ink">What this place offers</h2>

      <ul className="mt-4 grid gap-2.5 sm:grid-cols-2">
        {AMENITIES.map(({ key, label, Icon }) => {
          const included = property[key] === true;

          return (
            <li
              key={label}
              className={cn(
                'flex items-center gap-2.5 rounded-xl border px-3 py-2.5 text-sm',
                included ? 'border-line bg-surface text-ink' : 'border-dashed border-line text-ink-3',
              )}
            >
              <Icon className={cn('size-4', included ? 'text-cta' : 'text-ink-3')} />
              <span className={cn(!included && 'line-through decoration-line-strong')}>{label}</span>
              <span className="ml-auto" aria-hidden="true">
                {included ? <Check className="size-4 text-cta" /> : <Minus className="size-4" />}
              </span>
              <span className="sr-only">{included ? 'included' : 'not included'}</span>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
