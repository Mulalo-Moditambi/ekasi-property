import type { ComponentProps, ReactNode } from 'react';
import { cn } from './utils';

/**
 * Label wrapping its control, so no id plumbing is needed to stay accessible.
 */
export function Field({
  label,
  hint,
  error,
  className,
  children,
}: {
  label: ReactNode;
  hint?: ReactNode;
  error?: string | null;
  className?: string;
  children: ReactNode;
}) {
  return (
    <label className={cn('block', className)}>
      <span className="mb-1.5 block text-sm font-medium text-ink">{label}</span>
      {children}
      {hint && !error && <span className="mt-1 block text-xs text-ink-3">{hint}</span>}
      {error && <span className="mt-1 block text-xs font-medium text-danger">{error}</span>}
    </label>
  );
}

export function Textarea({ className, ...props }: ComponentProps<'textarea'>) {
  return (
    <textarea
      className={cn(
        'w-full rounded-lg border border-line bg-surface px-3 py-2 text-sm text-ink',
        'placeholder:text-ink-3 outline-none transition-[color,box-shadow,border-color]',
        'focus-visible:border-cta focus-visible:ring-2 focus-visible:ring-cta/25',
        'disabled:cursor-not-allowed disabled:opacity-50',
        className,
      )}
      {...props}
    />
  );
}

/** Inline status message — used for form-level errors and confirmations. */
export function Notice({
  tone = 'error',
  children,
  className,
}: {
  tone?: 'error' | 'success' | 'info';
  children: ReactNode;
  className?: string;
}) {
  return (
    <p
      role={tone === 'error' ? 'alert' : 'status'}
      className={cn(
        'rounded-lg border px-3 py-2 text-sm font-medium',
        tone === 'error' && 'border-danger/30 bg-danger-soft text-danger',
        tone === 'success' && 'border-cta-line bg-cta-soft text-cta',
        tone === 'info' && 'border-line bg-surface-2 text-ink-2',
        className,
      )}
    >
      {children}
    </p>
  );
}
