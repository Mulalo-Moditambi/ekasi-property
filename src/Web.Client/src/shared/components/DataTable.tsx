import type { ReactNode } from 'react';
import { ArrowDown, ArrowUp } from 'lucide-react';
import { cn } from '../ui/utils';

export interface DataTableColumn<T> {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
  sortable?: boolean;
  align?: 'left' | 'right' | 'center';
}

interface DataTableProps<T> {
  columns: DataTableColumn<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  sort?: { key: string; direction: 'asc' | 'desc' };
  onSort?: (key: string) => void;
  emptyMessage?: string;
}

const ALIGN = {
  left: 'text-left',
  right: 'text-right',
  center: 'text-center',
} as const;

export function DataTable<T>({ columns, rows, rowKey, sort, onSort, emptyMessage }: DataTableProps<T>) {
  if (rows.length === 0) {
    return (
      <div className="rounded-2xl border border-dashed border-line-strong bg-surface px-6 py-12 text-center text-sm text-ink-2">
        {emptyMessage ?? 'Nothing to show yet.'}
      </div>
    );
  }

  return (
    // Wide tables scroll inside their own container rather than the page.
    <div className="overflow-x-auto rounded-2xl border border-line bg-surface shadow-sm">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b border-line">
            {columns.map((column) => (
              <th
                key={column.key}
                scope="col"
                className={cn(
                  'whitespace-nowrap px-4 py-3 text-2xs font-semibold uppercase tracking-wider text-ink-3',
                  ALIGN[column.align ?? 'left'],
                )}
              >
                {column.sortable ? (
                  <button
                    type="button"
                    onClick={() => onSort?.(column.key)}
                    className="inline-flex items-center gap-1 rounded uppercase tracking-wider outline-none transition-colors hover:text-ink focus-visible:outline-2 focus-visible:outline-cta"
                  >
                    {column.header}
                    {sort?.key === column.key &&
                      (sort.direction === 'asc' ? (
                        <ArrowUp className="size-3" />
                      ) : (
                        <ArrowDown className="size-3" />
                      ))}
                  </button>
                ) : (
                  column.header
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-line">
          {rows.map((row) => (
            <tr key={rowKey(row)} className="transition-colors hover:bg-surface-2">
              {columns.map((column) => (
                <td
                  key={column.key}
                  className={cn('px-4 py-3 align-middle text-ink-2', ALIGN[column.align ?? 'left'])}
                >
                  {column.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
