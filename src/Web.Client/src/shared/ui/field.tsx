import * as SeparatorPrimitive from '@radix-ui/react-separator';
import type { ComponentProps, ReactNode } from 'react';
import { cn } from './utils';

export function Separator({
  className,
  orientation = 'horizontal',
  ...props
}: ComponentProps<typeof SeparatorPrimitive.Root>) {
  return (
    <SeparatorPrimitive.Root
      decorative
      orientation={orientation}
      className={cn(
        'shrink-0 bg-line',
        orientation === 'horizontal' ? 'h-px w-full' : 'h-full w-px',
        className,
      )}
      {...props}
    />
  );
}

/** A labelled block in the filter rail. */
export function FilterGroup({
  label,
  action,
  children,
}: {
  label: string;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="border-b border-line px-4 py-3.5 last:border-b-0">
      <div className="mb-2.5 flex items-center justify-between gap-2">
        <h3 className="text-2xs font-semibold uppercase tracking-wider text-ink-3">{label}</h3>
        {action}
      </div>
      {children}
    </section>
  );
}
