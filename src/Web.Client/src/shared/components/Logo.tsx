import { Link } from 'react-router-dom';
import { cn } from '../ui/utils';

/**
 * The mark: a roofline over an arched doorway. The door is the signature —
 * "own entrance" is the thing these listings actually sell, and it keeps the
 * glyph from being another generic house icon. Drawn on a 24 grid with round
 * joins so it stays legible down to 16px.
 */
export function LogoGlyph({ className }: { className?: string }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2.2}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className={className}
    >
      <path d="M4.2 11.4 12 4.8 19.8 11.4" />
      <path d="M6.6 12.8V19.9h10.8v-7.1" />
      <path d="M9.8 19.9v-3.3a2.2 2.2 0 0 1 4.4 0v3.3" />
    </svg>
  );
}

/** The glyph in its brand tile — the app's avatar wherever a compact mark is needed. */
export function LogoMark({ className }: { className?: string }) {
  return (
    <span
      className={cn(
        'grid size-8 shrink-0 place-items-center rounded-lg bg-brand text-brand-fg',
        className,
      )}
    >
      <LogoGlyph className="size-[18px]" />
    </span>
  );
}

/** Mark plus wordmark, linking home. Used in the header and the footer. */
export function Wordmark({
  className,
  /** Below 360px the header can't fit the name and the CTA — the mark alone carries it. */
  hideNameOnNarrow = false,
}: {
  className?: string;
  hideNameOnNarrow?: boolean;
}) {
  return (
    <Link
      to="/"
      aria-label="Ekasi Property — home"
      className={cn(
        'flex items-center gap-2.5 rounded-md outline-none',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand',
        className,
      )}
    >
      <LogoMark />
      <span
        className={cn(
          'text-[15px] font-semibold tracking-tight text-ink',
          hideNameOnNarrow && 'hidden min-[360px]:inline',
        )}
      >
        ekasi<span className="text-ink-3">property</span>
      </span>
    </Link>
  );
}
