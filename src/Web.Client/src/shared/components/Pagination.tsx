import { ChevronIcon } from './icons';

interface PaginationProps {
  page: number;
  totalPages: number;
  disabled?: boolean;
  onPageChange: (page: number) => void;
}

/** Windowed page numbers: 1 … 4 [5] 6 … 12, with single-step gaps filled in. */
function pageWindow(page: number, total: number): (number | 'gap')[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  const anchors = [...new Set([1, page - 1, page, page + 1, total])]
    .filter((p) => p >= 1 && p <= total)
    .sort((a, b) => a - b);

  const out: (number | 'gap')[] = [];
  let previous = 0;

  for (const p of anchors) {
    if (p - previous === 2) {
      out.push(previous + 1);
    } else if (p - previous > 2) {
      out.push('gap');
    }
    out.push(p);
    previous = p;
  }

  return out;
}

export function Pagination({ page, totalPages, disabled, onPageChange }: PaginationProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <nav className="pagination" aria-label="Pagination">
      <button
        type="button"
        className="page-btn"
        disabled={disabled || page <= 1}
        aria-label="Previous page"
        onClick={() => onPageChange(page - 1)}
      >
        <ChevronIcon direction="left" size={15} />
      </button>
      {pageWindow(page, totalPages).map((p, index) =>
        p === 'gap' ? (
          <span key={`gap-${index}`} className="page-gap" aria-hidden="true">
            …
          </span>
        ) : (
          <button
            key={p}
            type="button"
            className={p === page ? 'page-btn active' : 'page-btn'}
            disabled={disabled}
            aria-current={p === page ? 'page' : undefined}
            aria-label={`Page ${p}`}
            onClick={() => {
              if (p !== page) onPageChange(p);
            }}
          >
            {p}
          </button>
        ),
      )}
      <button
        type="button"
        className="page-btn"
        disabled={disabled || page >= totalPages}
        aria-label="Next page"
        onClick={() => onPageChange(page + 1)}
      >
        <ChevronIcon direction="right" size={15} />
      </button>
    </nav>
  );
}
