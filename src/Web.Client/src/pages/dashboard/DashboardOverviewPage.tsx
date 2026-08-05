import { useMemo } from 'react';
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { List, Users } from 'lucide-react';
import { paths } from '../../app/routes/paths';
import { useMyProperties, useMyPropertyStatusCounts } from '../../features/properties/queries';
import { useMyLeads } from '../../features/inquiries/queries';
import { StatCard } from '../../shared/components/StatCard';
import { errorMessage } from '../../shared/api/queryClient';
import { priceParts } from '../../shared/format';
import { Notice } from '../../shared/ui/form';
import { Skeleton } from '../../shared/ui/skeleton';
import { PropertyStatus, propertyStatusLabels } from '../../features/properties/types';

const STATUSES = [
  PropertyStatus.Listed,
  PropertyStatus.Rented,
  PropertyStatus.Sold,
  PropertyStatus.Withdrawn,
];

/** Status bar colours, drawn from the semantic tokens rather than raw hexes. */
const STATUS_COLORS: Record<PropertyStatus, string> = {
  [PropertyStatus.Listed]: 'var(--c-cta)',
  [PropertyStatus.Rented]: 'var(--c-info)',
  [PropertyStatus.Sold]: 'var(--c-violet)',
  [PropertyStatus.Withdrawn]: 'var(--c-ink-3)',
};

export function DashboardOverviewPage() {
  const {
    counts: statusCounts,
    total: totalListings,
    isPending: countsPending,
    error: countsError,
  } = useMyPropertyStatusCounts();

  const { data: listings, isPending: listingsPending } = useMyProperties({ sort: 'newest' }, 1, 5);
  const { data: leads, isPending: leadsPending } = useMyLeads({}, 1, 5);

  const recentListings = useMemo(() => listings?.items ?? [], [listings]);
  const recentLeads = useMemo(() => leads?.items ?? [], [leads]);

  if (countsPending || listingsPending || leadsPending) {
    return (
      <div>
        <PageHeading title="Overview" subtitle="A snapshot of your listings and leads." />
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-hidden="true">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} className="h-24 rounded-2xl" />
          ))}
        </div>
      </div>
    );
  }

  if (countsError) {
    return (
      <div>
        <PageHeading title="Overview" subtitle="A snapshot of your listings and leads." />
        <Notice>{errorMessage(countsError, 'Could not load your dashboard')}</Notice>
      </div>
    );
  }

  return (
    <div>
      <PageHeading title="Overview" subtitle="A snapshot of your listings and leads." />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          label="Active listings"
          value={statusCounts[PropertyStatus.Listed]}
          icon={<List />}
          accent
        />
        <StatCard label="Rented" value={statusCounts[PropertyStatus.Rented]} icon={<List />} />
        <StatCard label="Sold" value={statusCounts[PropertyStatus.Sold]} icon={<List />} />
        <StatCard label="Total leads" value={leads?.totalCount ?? 0} icon={<Users />} />
      </div>

      <Section title="Listings by status">
        {totalListings === 0 ? (
          <p className="text-sm text-ink-3">You haven&apos;t listed anything yet.</p>
        ) : (
          <>
            {/* A single proportional bar reads faster than a donut at this size. */}
            <div className="flex h-2.5 w-full overflow-hidden rounded-full bg-surface-3">
              {STATUSES.filter((status) => statusCounts[status] > 0).map((status) => (
                <div
                  key={status}
                  style={{
                    width: `${(statusCounts[status] / totalListings) * 100}%`,
                    background: STATUS_COLORS[status],
                  }}
                  title={`${propertyStatusLabels[status]}: ${statusCounts[status]}`}
                />
              ))}
            </div>
            <ul className="mt-4 flex flex-wrap gap-x-6 gap-y-2">
              {STATUSES.map((status) => (
                <li key={status} className="flex items-center gap-2 text-sm text-ink-2">
                  <span
                    className="size-2.5 rounded-full"
                    style={{ background: STATUS_COLORS[status] }}
                    aria-hidden="true"
                  />
                  {propertyStatusLabels[status]}
                  <span className="tnum font-semibold text-ink">{statusCounts[status]}</span>
                </li>
              ))}
            </ul>
          </>
        )}
      </Section>

      <Section title="Recent listings" action={{ to: paths.manager.properties, label: 'View all' }}>
        {recentListings.length === 0 ? (
          <p className="text-sm text-ink-3">Nothing here yet.</p>
        ) : (
          <ul className="divide-y divide-line">
            {recentListings.map((listing) => {
              const { amount, period } = priceParts(listing.price, listing.listingType);
              return (
                <li key={listing.id}>
                  <Link
                    to={paths.properties.detail(listing.id)}
                    className="flex items-center justify-between gap-4 py-3 outline-none transition-colors hover:bg-surface-2 focus-visible:outline-2 focus-visible:outline-cta"
                  >
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-ink">{listing.title}</p>
                      <p className="truncate text-xs text-ink-3">{listing.township}</p>
                    </div>
                    <p className="tnum shrink-0 text-sm font-semibold text-ink">
                      {amount}
                      {period && <span className="font-normal text-ink-3"> /mo</span>}
                    </p>
                  </Link>
                </li>
              );
            })}
          </ul>
        )}
      </Section>

      <Section title="Recent leads" action={{ to: paths.manager.leads, label: 'View all' }}>
        {recentLeads.length === 0 ? (
          <p className="text-sm text-ink-3">No inquiries yet.</p>
        ) : (
          <ul className="divide-y divide-line">
            {recentLeads.map((lead) => (
              <li key={lead.id}>
                <Link
                  to={paths.properties.detail(lead.propertyId)}
                  className="flex items-center justify-between gap-4 py-3 outline-none transition-colors hover:bg-surface-2 focus-visible:outline-2 focus-visible:outline-cta"
                >
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-ink">{lead.name}</p>
                    <p className="truncate text-xs text-ink-3">{lead.propertyTitle}</p>
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </Section>
    </div>
  );
}

export function PageHeading({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="mb-6">
      <h1 className="text-2xl font-bold tracking-tight text-ink">{title}</h1>
      {subtitle && <p className="mt-1 text-sm text-ink-2">{subtitle}</p>}
    </div>
  );
}

function Section({
  title,
  action,
  children,
}: {
  title: string;
  action?: { to: string; label: string };
  children: ReactNode;
}) {
  return (
    <section className="mt-6 rounded-2xl border border-line bg-surface p-5 shadow-sm">
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 className="text-base font-semibold tracking-tight text-ink">{title}</h2>
        {action && (
          <Link
            to={action.to}
            className="rounded text-sm font-medium text-cta underline-offset-4 hover:underline focus-visible:outline-2 focus-visible:outline-cta"
          >
            {action.label}
          </Link>
        )}
      </div>
      {children}
    </section>
  );
}
