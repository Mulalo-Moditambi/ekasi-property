import { useCallback, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { paths } from '../../app/routes/paths';
import {
  lifecycleMessages,
  useListingLifecycle,
  useMyProperties,
} from '../../features/properties/queries';
import type { LifecycleAction } from '../../features/properties/queries';
import { ConfirmDialog } from '../../shared/components/ConfirmDialog';
import { DataTable } from '../../shared/components/DataTable';
import type { DataTableColumn } from '../../shared/components/DataTable';
import { useToast } from '../../shared/components/Toast';
import { errorMessage } from '../../shared/api/queryClient';
import { Badge } from '../../shared/ui/badge';
import { Button } from '../../shared/ui/button';
import { Notice } from '../../shared/ui/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../../shared/ui/select';
import { Skeleton } from '../../shared/ui/skeleton';
import type { MyProperty } from '../../features/properties/types';
import {
  ListingType,
  PropertyStatus,
  formatPrice,
  propertyStatusLabels,
  propertyTypeLabels,
} from '../../features/properties/types';

/** Maps listing status onto the design system's semantic badge tones. */
const STATUS_TONE = {
  [PropertyStatus.Listed]: 'cta',
  [PropertyStatus.Rented]: 'info',
  [PropertyStatus.Sold]: 'violet',
  [PropertyStatus.Withdrawn]: 'neutral',
} as const;

export function ManageListingsPage() {
  const { notify } = useToast();
  const [statusFilter, setStatusFilter] = useState<PropertyStatus | 'all'>('all');
  const [pendingDelete, setPendingDelete] = useState<MyProperty | null>(null);

  const filters = useMemo(
    () => ({ status: statusFilter === 'all' ? undefined : statusFilter, sort: 'newest' as const }),
    [statusFilter],
  );

  // The previous fetch had no `catch`: any failure left the spinner running
  // forever and threw an unhandled rejection. The query surfaces it instead.
  const { data: result, isPending, error } = useMyProperties(filters);
  const lifecycle = useListingLifecycle();

  const run = useCallback(
    (id: string, action: LifecycleAction) => {
      lifecycle.mutate(
        { id, action },
        {
          onSuccess: () => notify(lifecycleMessages[action]),
          onError: (cause) => notify(errorMessage(cause, 'Action failed'), 'error'),
        },
      );
    },
    [lifecycle, notify],
  );

  // `variables` is the in-flight mutation's input, which replaces the manual
  // busyId state the old hook carried.
  const busyId = lifecycle.isPending ? lifecycle.variables?.id : undefined;

  const columns = useMemo<DataTableColumn<MyProperty>[]>(
    () => [
      {
        key: 'title',
        header: 'Listing',
        render: (row) => (
          <div className="flex min-w-0 items-center gap-3">
            <div className="size-11 shrink-0 overflow-hidden rounded-lg bg-surface-3">
              {row.coverImageUrl && (
                <img src={row.coverImageUrl} alt="" loading="lazy" className="size-full object-cover" />
              )}
            </div>
            <div className="min-w-0">
              <Link
                to={paths.properties.detail(row.id)}
                className="block truncate font-semibold text-ink underline-offset-4 hover:underline"
              >
                {row.title}
              </Link>
              <p className="truncate text-xs text-ink-3">
                {row.township} · {propertyTypeLabels[row.propertyType]}
              </p>
            </div>
          </div>
        ),
      },
      {
        key: 'status',
        header: 'Status',
        render: (row) => (
          <Badge tone={STATUS_TONE[row.status]}>{propertyStatusLabels[row.status]}</Badge>
        ),
      },
      {
        key: 'price',
        header: 'Price',
        align: 'right',
        render: (row) => (
          <span className="tnum whitespace-nowrap font-semibold text-ink">
            {formatPrice(row.price, row.listingType)}
          </span>
        ),
      },
      {
        key: 'actions',
        header: 'Actions',
        render: (row) => {
          const busy = busyId === row.id;
          const isListed = row.status === PropertyStatus.Listed;
          const canRelist =
            row.status === PropertyStatus.Rented || row.status === PropertyStatus.Withdrawn;

          return (
            <div className="flex flex-wrap items-center justify-end gap-1.5">
              {isListed && row.listingType === ListingType.Rent && (
                <Button
                  variant="outline"
                  size="sm"
                  disabled={busy}
                  onClick={() => run(row.id, 'markRented')}
                >
                  Mark rented
                </Button>
              )}
              {isListed && row.listingType === ListingType.Sale && (
                <Button variant="outline" size="sm" disabled={busy} onClick={() => run(row.id, 'markSold')}>
                  Mark sold
                </Button>
              )}
              {isListed && (
                <Button variant="outline" size="sm" disabled={busy} onClick={() => run(row.id, 'withdraw')}>
                  Withdraw
                </Button>
              )}
              {canRelist && (
                <Button variant="outline" size="sm" disabled={busy} onClick={() => run(row.id, 'relist')}>
                  Relist
                </Button>
              )}

              <Button variant="outline" size="sm" asChild>
                <Link to={paths.manager.editProperty(row.id)}>
                  <Pencil className="size-3.5" />
                  Edit
                </Link>
              </Button>

              {/* Subdued in the table — the confirm dialog carries the loud red. */}
              <Button
                variant="outline"
                size="sm"
                disabled={busy}
                onClick={() => setPendingDelete(row)}
                className="border-danger/30 text-danger hover:bg-danger-soft hover:text-danger"
              >
                <Trash2 className="size-3.5" />
                Delete
              </Button>
            </div>
          );
        },
      },
    ],
    [busyId, run],
  );

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-ink">Listings</h1>
          <p className="mt-1 text-sm text-ink-2">{result ? `${result.totalCount} total` : ''}</p>
        </div>
        <Button variant="cta" asChild>
          <Link to={paths.manager.newProperty}>
            <Plus className="size-4" />
            New listing
          </Link>
        </Button>
      </div>

      <div className="mb-4">
        <Select
          value={statusFilter === 'all' ? 'all' : String(statusFilter)}
          onValueChange={(value) =>
            setStatusFilter(value === 'all' ? 'all' : (Number(value) as PropertyStatus))
          }
        >
          <SelectTrigger aria-label="Filter by status" className="h-9 w-48 rounded-lg">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All statuses</SelectItem>
            {Object.entries(propertyStatusLabels).map(([value, label]) => (
              <SelectItem key={value} value={value}>
                {label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {error && <Notice className="mb-4">{errorMessage(error, 'Could not load your listings')}</Notice>}

      {isPending ? (
        <div className="space-y-2" aria-hidden="true">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-16 rounded-xl" />
          ))}
        </div>
      ) : (
        <DataTable
          columns={columns}
          rows={result?.items ?? []}
          rowKey={(row) => row.id}
          emptyMessage="You haven't listed anything yet. Create your first listing to get started."
        />
      )}

      {pendingDelete && (
        <ConfirmDialog
          title="Delete this listing?"
          message={`"${pendingDelete.title}" and its photos will be permanently removed.`}
          confirmLabel="Delete"
          danger
          busy={busyId === pendingDelete.id}
          onConfirm={() => {
            const target = pendingDelete;
            setPendingDelete(null);
            run(target.id, 'remove');
          }}
          onCancel={() => setPendingDelete(null)}
        />
      )}
    </div>
  );
}
