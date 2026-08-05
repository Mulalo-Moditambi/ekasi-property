import { cva } from 'class-variance-authority';
import type { VariantProps } from 'class-variance-authority';
import type { ComponentProps } from 'react';
import { cn } from './utils';

const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded-sm border px-1.5 py-0.5 text-2xs font-semibold ' +
    'uppercase tracking-wide whitespace-nowrap',
  {
    variants: {
      tone: {
        neutral: 'border-line bg-surface-3 text-ink-2',
        brand: 'border-brand-line bg-brand-soft text-brand',
        /** Solid navy — legible over photography, where a tinted badge is not. */
        solid: 'border-transparent bg-brand text-brand-fg shadow-sm',
        /** Solid emerald — the conversion accent, for "new"/"active" style flags. */
        cta: 'border-transparent bg-cta text-cta-fg shadow-sm',
        ok: 'border-transparent bg-ok-soft text-ok',
        info: 'border-transparent bg-info-soft text-info',
        violet: 'border-transparent bg-violet-soft text-violet',
        warn: 'border-transparent bg-warn-soft text-warn',
        danger: 'border-transparent bg-danger-soft text-danger',
      },
    },
    defaultVariants: { tone: 'neutral' },
  },
);

export type BadgeProps = ComponentProps<'span'> & VariantProps<typeof badgeVariants>;

export function Badge({ className, tone, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ tone }), className)} {...props} />;
}
