import * as ToggleGroupPrimitive from '@radix-ui/react-toggle-group';
import type { ComponentProps } from 'react';
import { cn } from './utils';

/**
 * Single-choice segmented control. Unlike a raw toggle group it never allows an
 * empty selection — deselecting the active item is a no-op, so filters like
 * "Rent / Buy / Any" always resolve to a value.
 */
// `dir` and `defaultValue` are narrower on the Radix root than on a plain div.
type SegmentedProps = Omit<ComponentProps<'div'>, 'defaultValue' | 'onChange' | 'dir'> & {
  value: string;
  onValueChange: (value: string) => void;
};

export function Segmented({ className, value, onValueChange, ...props }: SegmentedProps) {
  return (
    <ToggleGroupPrimitive.Root
      type="single"
      value={value}
      onValueChange={(next) => next && onValueChange(next)}
      className={cn('inline-flex items-center gap-0.5 rounded-lg bg-surface-3 p-0.5', className)}
      {...props}
    />
  );
}

export function SegmentedItem({
  className,
  ...props
}: ComponentProps<typeof ToggleGroupPrimitive.Item>) {
  return (
    <ToggleGroupPrimitive.Item
      className={cn(
        // Inner radius sits inside the 5px track less its 2px padding.
        'inline-flex h-7 items-center justify-center gap-1.5 rounded-[3px] px-2.5',
        'text-xs font-medium text-ink-2 transition-colors outline-none',
        'hover:text-ink focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-brand',
        'data-[state=on]:bg-surface data-[state=on]:text-ink data-[state=on]:shadow-row',
        "[&_svg:not([class*='size-'])]:size-3.5",
        className,
      )}
      {...props}
    />
  );
}
