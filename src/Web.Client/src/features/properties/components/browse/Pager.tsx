import { memo } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '../../../../shared/ui/button';

/** Windowed page numbers — first, last, and a run around the current page. */
function pageWindow(current: number, total: number): (number | 'gap')[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, index) => index + 1);
  }

  const pages = new Set([1, total, current, current - 1, current + 1]);
  const visible = [...pages].filter((p) => p >= 1 && p <= total).sort((a, b) => a - b);

  return visible.flatMap((p, index) =>
    index > 0 && p - visible[index - 1] > 1 ? (['gap', p] as (number | 'gap')[]) : [p],
  );
}

interface PagerProps {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
}

export const Pager = memo(function Pager({ page, totalPages, onChange }: PagerProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <nav className="mt-7 flex items-center justify-center gap-1" aria-label="Pagination">
      <Button
        variant="outline"
        size="icon-sm"
        disabled={page <= 1}
        onClick={() => onChange(page - 1)}
        aria-label="Previous page"
      >
        <ChevronLeft />
      </Button>

      {pageWindow(page, totalPages).map((entry, index) =>
        entry === 'gap' ? (
          <span key={`gap-${index}`} className="px-1 text-sm text-ink-3">
            …
          </span>
        ) : (
          <Button
            key={entry}
            variant={entry === page ? 'primary' : 'ghost'}
            size="icon-sm"
            className="tnum text-xs"
            aria-current={entry === page ? 'page' : undefined}
            aria-label={`Page ${entry}`}
            onClick={() => onChange(entry)}
          >
            {entry}
          </Button>
        ),
      )}

      <Button
        variant="outline"
        size="icon-sm"
        disabled={page >= totalPages}
        onClick={() => onChange(page + 1)}
        aria-label="Next page"
      >
        <ChevronRight />
      </Button>
    </nav>
  );
});
