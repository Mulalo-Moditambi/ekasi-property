import type { ComponentProps } from 'react';
import { cn } from './utils';

export function Input({ className, ...props }: ComponentProps<'input'>) {
  return (
    <input
      className={cn(
        'h-9 w-full rounded-md border border-line bg-surface px-2.5 text-sm text-ink',
        'placeholder:text-ink-3 outline-none transition-[color,box-shadow,border-color]',
        'focus-visible:border-brand focus-visible:ring-2 focus-visible:ring-brand/25',
        'disabled:cursor-not-allowed disabled:opacity-50',
        '[&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none',
        className,
      )}
      {...props}
    />
  );
}
