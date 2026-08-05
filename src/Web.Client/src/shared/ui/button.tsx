import { Slot } from '@radix-ui/react-slot';
import { cva } from 'class-variance-authority';
import type { VariantProps } from 'class-variance-authority';
import type { ComponentProps } from 'react';
import { cn } from './utils';

const buttonVariants = cva(
  'inline-flex shrink-0 items-center justify-center gap-1.5 whitespace-nowrap rounded-lg font-medium ' +
    'transition-[color,background-color,box-shadow] duration-200 ease-[cubic-bezier(0.4,0,0.2,1)] ' +
    'outline-none focus-visible:outline-2 focus-visible:outline-offset-2 ' +
    'focus-visible:outline-cta disabled:pointer-events-none disabled:opacity-45 ' +
    'active:scale-[0.98] ' +
    "[&_svg]:pointer-events-none [&_svg:not([class*='size-'])]:size-4",
  {
    variants: {
      variant: {
        /** High-intent conversion actions only — the 5% accent. */
        cta: 'bg-cta text-cta-fg shadow-sm hover:bg-cta-hover hover:shadow-md',
        /** Navy. Primary but non-conversion: nav, headers, secondary commits. */
        primary: 'bg-brand text-brand-fg hover:bg-brand-hover',
        outline: 'border border-line-strong bg-surface text-ink hover:bg-surface-3',
        subtle: 'bg-surface-3 text-ink hover:bg-line',
        ghost: 'text-ink-2 hover:bg-surface-3 hover:text-ink',
        danger: 'bg-danger text-white hover:brightness-110',
        link: 'text-cta underline-offset-4 hover:underline',
      },
      size: {
        sm: 'h-8 px-2.5 text-xs',
        md: 'h-9 px-3.5 text-sm',
        lg: 'h-11 px-5 text-sm',
        icon: 'size-9',
        'icon-sm': 'size-8',
      },
    },
    defaultVariants: { variant: 'primary', size: 'md' },
  },
);

export type ButtonProps = ComponentProps<'button'> &
  VariantProps<typeof buttonVariants> & { asChild?: boolean };

export function Button({ className, variant, size, asChild = false, ...props }: ButtonProps) {
  const Comp = asChild ? Slot : 'button';

  return <Comp className={cn(buttonVariants({ variant, size }), className)} {...props} />;
}

export { buttonVariants };
