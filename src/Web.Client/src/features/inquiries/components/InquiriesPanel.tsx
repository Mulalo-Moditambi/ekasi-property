import { errorMessage } from '../../../shared/api/queryClient';
import { Notice } from '../../../shared/ui/form';
import { Skeleton } from '../../../shared/ui/skeleton';
import { useInquiries } from '../queries';

export function InquiriesPanel({ propertyId }: { propertyId: string }) {
  const { data: inquiries, isPending, error } = useInquiries(propertyId);

  return (
    <section className="rounded-2xl border border-line bg-surface p-5 shadow-sm">
      <h2 className="text-lg font-semibold tracking-tight text-ink">Inquiries</h2>

      {error && <Notice className="mt-3">{errorMessage(error, 'Could not load inquiries')}</Notice>}

      {!error && isPending && (
        <div className="mt-4 space-y-3" aria-hidden="true">
          {Array.from({ length: 2 }, (_, i) => (
            <Skeleton key={i} className="h-20 rounded-xl" />
          ))}
        </div>
      )}

      {inquiries && inquiries.length === 0 && (
        <p className="mt-2 text-sm text-ink-3">
          No inquiries yet. When someone contacts you about this listing, it shows up here.
        </p>
      )}

      {inquiries && inquiries.length > 0 && (
        <ul className="mt-4 divide-y divide-line">
          {inquiries.map((inquiry) => (
            <li key={inquiry.id} className="py-4 first:pt-0 last:pb-0">
              <div className="flex flex-wrap items-baseline justify-between gap-2">
                <strong className="text-sm font-semibold text-ink">{inquiry.name}</strong>
                <span className="text-xs text-ink-3">
                  {new Date(inquiry.createdAt).toLocaleString()}
                </span>
              </div>
              <p className="mt-1 flex flex-wrap items-center gap-x-2 text-xs">
                <a
                  href={`mailto:${inquiry.email}`}
                  className="text-cta underline-offset-4 hover:underline"
                >
                  {inquiry.email}
                </a>
                {inquiry.phone && (
                  <>
                    <span className="text-ink-3">·</span>
                    <a
                      href={`tel:${inquiry.phone}`}
                      className="text-cta underline-offset-4 hover:underline"
                    >
                      {inquiry.phone}
                    </a>
                  </>
                )}
              </p>
              <p className="mt-2 text-sm leading-relaxed text-ink-2">{inquiry.message}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
