import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Pencil } from 'lucide-react';
import { paths } from '../../../../app/routes/paths';
import { ConfirmDialog } from '../../../../shared/components/ConfirmDialog';
import { useToast } from '../../../../shared/components/Toast';
import { errorMessage } from '../../../../shared/api/queryClient';
import { Button } from '../../../../shared/ui/button';
import { lifecycleMessages, useListingLifecycle } from '../../queries';
import type { LifecycleAction } from '../../queries';
import { ListingType, PropertyStatus } from '../../types';
import type { PropertyDetail } from '../../types';

/**
 * Owner-only controls for one listing. Which transitions are legal is decided
 * here rather than by the caller, so the detail page and the portfolio table
 * can't disagree about, say, whether a sold listing can be relisted.
 */
export function OwnerPanel({ property }: { property: PropertyDetail }) {
  const navigate = useNavigate();
  const { notify } = useToast();
  const lifecycle = useListingLifecycle();
  const [confirmingDelete, setConfirmingDelete] = useState(false);

  const isListed = property.status === PropertyStatus.Listed;
  const canRelist =
    property.status === PropertyStatus.Rented || property.status === PropertyStatus.Withdrawn;

  function run(action: LifecycleAction, onDone?: () => void) {
    lifecycle.mutate(
      { id: property.id, action },
      {
        onSuccess: () => {
          notify(lifecycleMessages[action]);
          onDone?.();
        },
        onError: (error) => notify(errorMessage(error, 'Action failed'), 'error'),
      },
    );
  }

  const busy = lifecycle.isPending;

  return (
    <section>
      <h2 className="text-base font-semibold tracking-tight text-ink">Manage your listing</h2>

      <div className="mt-4 flex flex-col gap-2">
        {isListed && property.listingType === ListingType.Rent && (
          <Button variant="cta" disabled={busy} onClick={() => run('markRented')}>
            Mark as rented
          </Button>
        )}

        {isListed && property.listingType === ListingType.Sale && (
          <Button variant="cta" disabled={busy} onClick={() => run('markSold')}>
            Mark as sold
          </Button>
        )}

        {isListed && (
          <Button variant="outline" disabled={busy} onClick={() => run('withdraw')}>
            Withdraw
          </Button>
        )}

        {canRelist && (
          <Button variant="cta" disabled={busy} onClick={() => run('relist')}>
            Relist
          </Button>
        )}

        <Button variant="outline" asChild>
          <Link to={paths.manager.editProperty(property.id)}>
            <Pencil />
            Edit listing &amp; photos
          </Link>
        </Button>

        <Button
          variant="ghost"
          disabled={busy}
          onClick={() => setConfirmingDelete(true)}
          className="text-danger hover:bg-danger-soft hover:text-danger"
        >
          Delete listing
        </Button>
      </div>

      {confirmingDelete && (
        <ConfirmDialog
          title="Delete this listing?"
          message="This permanently removes the listing and its photos. This can't be undone."
          confirmLabel="Delete"
          danger
          busy={busy}
          onConfirm={() => {
            setConfirmingDelete(false);
            run('remove', () => navigate(paths.manager.properties));
          }}
          onCancel={() => setConfirmingDelete(false)}
        />
      )}
    </section>
  );
}
