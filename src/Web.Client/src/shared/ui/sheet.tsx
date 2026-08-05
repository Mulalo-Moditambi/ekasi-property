import * as DialogPrimitive from '@radix-ui/react-dialog';
import { X } from 'lucide-react';
import type { ComponentProps } from 'react';
import { cn } from './utils';

export const Sheet = DialogPrimitive.Root;
export const SheetTrigger = DialogPrimitive.Trigger;
export const SheetClose = DialogPrimitive.Close;
export const SheetTitle = DialogPrimitive.Title;
export const SheetDescription = DialogPrimitive.Description;

export function SheetContent({
  className,
  children,
  side = 'left',
  ...props
}: ComponentProps<typeof DialogPrimitive.Content> & { side?: 'left' | 'right' | 'bottom' }) {
  return (
    <DialogPrimitive.Portal>
      <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-black/45 backdrop-blur-[1px]" />
      <DialogPrimitive.Content
        className={cn(
          'fixed z-50 flex flex-col bg-surface shadow-pop outline-none',
          side === 'left' && 'inset-y-0 left-0 w-[88vw] max-w-sm border-r border-line',
          side === 'right' && 'inset-y-0 right-0 w-[88vw] max-w-sm border-l border-line',
          side === 'bottom' && 'inset-x-0 bottom-0 max-h-[85vh] rounded-t-xl border-t border-line',
          className,
        )}
        {...props}
      >
        {children}
        <DialogPrimitive.Close
          className={cn(
            'absolute right-3 top-3 rounded-md p-1 text-ink-3 transition-colors',
            'hover:bg-surface-3 hover:text-ink focus-visible:outline-2 focus-visible:outline-brand',
          )}
        >
          <X className="size-4" />
          <span className="sr-only">Close</span>
        </DialogPrimitive.Close>
      </DialogPrimitive.Content>
    </DialogPrimitive.Portal>
  );
}
