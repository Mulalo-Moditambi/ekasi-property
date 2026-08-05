import type { ReactNode } from 'react';
import { cn } from '../ui/utils';

interface StatCardProps {
  label: string;
  value: string | number;
  icon?: ReactNode;
  /** Emphasises the primary metric in a row of tiles. */
  accent?: boolean;
}

export function StatCard({ label, value, icon, accent }: StatCardProps) {
  return (
    <div
      className={cn(
        'rounded-2xl border bg-surface p-4 shadow-sm',
        accent ? 'border-cta-line' : 'border-line',
      )}
    >
      <div className="flex items-center gap-2 text-ink-3">
        {icon && <span className={cn('[&_svg]:size-4', accent && 'text-cta')}>{icon}</span>}
        <span className="text-2xs font-semibold uppercase tracking-wider">{label}</span>
      </div>
      <p className="tnum mt-2 text-2xl font-bold tracking-tight text-ink">{value}</p>
    </div>
  );
}
