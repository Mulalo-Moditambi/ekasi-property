import { ExternalLink, MapPin } from 'lucide-react';
import { Button } from '../../../../shared/ui/button';
import { usePropertyDetailContext } from './context';

/**
 * There is no map here on purpose. The API stores a postal address and no
 * coordinates, so an embedded map would have to geocode client-side on every
 * view — slow, rate-limited, and wrong often enough to send someone to the
 * wrong street. Handing the address to the user's own map app is accurate and
 * costs nothing. Wiring a real map is a backend change (lat/lng on Property).
 */
export function LocationTab() {
  const { property } = usePropertyDetailContext();

  const lines = [
    property.street,
    property.township,
    `${property.city}, ${property.province}`,
    property.postalCode,
  ].filter(Boolean);

  const directions = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(
    lines.join(', '),
  )}`;

  return (
    <div className="pt-6">
      <h2 className="text-lg font-semibold tracking-tight text-ink">Where it is</h2>

      <div className="mt-4 flex flex-wrap items-start gap-5 rounded-2xl border border-line bg-surface p-5">
        <span className="grid size-10 shrink-0 place-items-center rounded-xl bg-cta-soft text-cta">
          <MapPin className="size-5" />
        </span>

        <address className="min-w-0 not-italic">
          {lines.map((line) => (
            <p key={line} className="text-[15px] leading-relaxed text-ink">
              {line}
            </p>
          ))}
        </address>

        <Button variant="outline" size="sm" className="ml-auto" asChild>
          <a href={directions} target="_blank" rel="noreferrer noopener">
            Get directions
            <ExternalLink />
            <span className="sr-only"> (opens in a new tab)</span>
          </a>
        </Button>
      </div>

      <p className="mt-3 text-xs leading-relaxed text-ink-3">
        Always view a property in person before paying a deposit.
      </p>
    </div>
  );
}
