import { Bath, BedDouble } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { usePropertyDetailContext } from './context';

function Spec({ Icon, value, label }: { Icon: LucideIcon; value: number; label: string }) {
  return (
    <span className="flex items-center gap-2 text-[15px] text-ink-2">
      <Icon className="size-5 text-ink-3" />
      <span className="tnum font-semibold text-ink">{value}</span> {label}
    </span>
  );
}

export function OverviewTab() {
  const { property } = usePropertyDetailContext();

  return (
    <div className="pt-6">
      <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
        <Spec
          Icon={BedDouble}
          value={property.bedrooms}
          label={property.bedrooms === 1 ? 'bedroom' : 'bedrooms'}
        />
        <Spec
          Icon={Bath}
          value={property.bathrooms}
          label={property.bathrooms === 1 ? 'bathroom' : 'bathrooms'}
        />
      </div>

      <section className="mt-7">
        <h2 className="text-lg font-semibold tracking-tight text-ink">About this property</h2>
        <p className="mt-2 whitespace-pre-line text-[15px] leading-relaxed text-ink-2">
          {property.description}
        </p>
      </section>
    </div>
  );
}
