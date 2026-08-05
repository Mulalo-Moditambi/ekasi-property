import * as PopoverPrimitive from '@radix-ui/react-popover';
import type { ComponentProps } from 'react';
import { cn } from './utils';

export const Popover = PopoverPrimitive.Root;
export const PopoverTrigger = PopoverPrimitive.Trigger;

export function PopoverContent({
  className,
  align = 'start',
  sideOffset = 8,
  ...props
}: ComponentProps<typeof PopoverPrimitive.Content>) {
  return (
    <PopoverPrimitive.Portal>
      <PopoverPrimitive.Content
        align={align}
        sideOffset={sideOffset}
        className={cn(
          'z-50 rounded-2xl border border-line bg-surface p-4 text-ink shadow-pop outline-none',
          'duration-200 ease-[cubic-bezier(0.4,0,0.2,1)]',
          className,
        )}
        {...props}
      />
    </PopoverPrimitive.Portal>
  );
}
