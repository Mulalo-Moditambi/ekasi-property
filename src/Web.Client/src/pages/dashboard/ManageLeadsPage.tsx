import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { Check, Home, Inbox, Mail, Phone } from 'lucide-react';
import { paths } from '../../app/routes/paths';
import { useMyLeads } from '../../features/inquiries/queries';
import { useMyProperties } from '../../features/properties/queries';
import { isLeadRead, markLeadRead, markLeadsRead } from '../../features/inquiries/readTracking';
import { DataTable } from '../../shared/components/DataTable';
import type { DataTableColumn } from '../../shared/components/DataTable';
import { errorMessage } from '../../shared/api/queryClient';
import { useDebouncedInput, useUrlState } from '../../shared/hooks/useUrlState';
import { Button } from '../../shared/ui/button';
import { Notice } from '../../shared/ui/form';
import { Input } from '../../shared/ui/input';
import { Skeleton } from '../../shared/ui/skeleton';
import { cn } from '../../shared/ui/utils';
import type { Lead } from '../../features/inquiries/types';

interface LeadsState {
  propertyId: string;
  search: string;
}

/**
 * `search` lives in the URL so an owner can bookmark "leads matching X". The
 * selected property is in the URL too — it's the current view, not local UI
 * state, so refresh/back/forward and sharing all keep pointing at the same pane.
 */
const leadsCodec = {
  parse: (params: URLSearchParams): LeadsState => ({
    propertyId: params.get('listing') ?? '',
    search: params.get('q') ?? '',
  }),
  serialize: ({ propertyId, search }: LeadsState) => {
    const params = new URLSearchParams();
    if (propertyId) params.set('listing', propertyId);
    if (search) params.set('q', search);
    return params;
  },
};

/**
 * Read/unread is a client-only concept — the API has no status field on an
 * inquiry — so it is mirrored into React state to drive re-renders. Reading it
 * straight from localStorage during render (as this page used to) meant marking
 * one read required a manual `forceRerender` counter.
 */
function useReadLeads() {
  const [, bump] = useState(0);

  const isRead = useCallback((id: string) => isLeadRead(id), []);

  const markRead = useCallback((id: string) => {
    markLeadRead(id);
    bump((n) => n + 1);
  }, []);

  const markAllRead = useCallback((ids: string[]) => {
    markLeadsRead(ids);
    bump((n) => n + 1);
  }, []);

  return { isRead, markRead, markAllRead };
}

interface PropertyGroup {
  propertyId: string;
  propertyTitle: string;
  propertyTownship: string;
  leads: Lead[];
}

/** Newest lead first within a property, and properties ordered by their most recent lead. */
function groupByProperty(leads: Lead[]): PropertyGroup[] {
  const groups = new Map<string, PropertyGroup>();

  for (const lead of leads) {
    const existing = groups.get(lead.propertyId);
    if (existing) {
      existing.leads.push(lead);
    } else {
      groups.set(lead.propertyId, {
        propertyId: lead.propertyId,
        propertyTitle: lead.propertyTitle,
        propertyTownship: lead.propertyTownship,
        leads: [lead],
      });
    }
  }

  return [...groups.values()].sort(
    (a, b) => new Date(b.leads[0].createdAt).getTime() - new Date(a.leads[0].createdAt).getTime(),
  );
}

export function ManageLeadsPage() {
  const { state, setState } = useUrlState<LeadsState>(leadsCodec);
  const { propertyId: selectedId, search } = state;

  const commitSearch = useCallback(
    (next: string) => setState((current) => ({ ...current, search: next }), { replace: true }),
    [setState],
  );

  const [searchDraft, setSearchDraft] = useDebouncedInput(search, commitSearch);

  // Fetched unfiltered by property — the left pane is built from this same set,
  // so switching properties is instant and never refetches. 50 is the API's
  // hard cap (`GetMyLeadsQueryHandler.MaxPageSize`), so this is "as many as it
  // will give us in one page," not an arbitrary round number.
  const filters = useMemo(() => ({ search: search || undefined }), [search]);
  const { data: result, isPending, error } = useMyLeads(filters, 1, 50);

  // Leads carry the title/township but not a photo, so the cover image comes
  // from the owner's property list and is joined in below by id.
  const { data: properties } = useMyProperties({ sort: 'newest' }, 1, 50);
  const coverImages = useMemo(
    () => new Map((properties?.items ?? []).map((p) => [p.id, p.coverImageUrl])),
    [properties],
  );

  const groups = useMemo(() => groupByProperty(result?.items ?? []), [result]);
  const { isRead, markRead, markAllRead } = useReadLeads();

  const selectProperty = useCallback(
    (propertyId: string) => setState((current) => ({ ...current, propertyId }), { replace: true }),
    [setState],
  );

  // Default to the property with the most recent activity once groups load,
  // rather than leaving the right pane blank on first visit.
  useEffect(() => {
    if (!selectedId && groups.length > 0) {
      selectProperty(groups[0].propertyId);
    }
  }, [selectedId, groups, selectProperty]);

  const selected = groups.find((g) => g.propertyId === selectedId) ?? groups[0];
  const selectedLeads = selected?.leads ?? [];

  // Recomputed every render rather than memoised: it reads the localStorage-backed
  // read-set, which React can't see, and marking one read re-renders anyway.
  const unreadCounts = new Map(groups.map((g) => [g.propertyId, g.leads.filter((l) => !isRead(l.id)).length]));
  const selectedUnread = selected ? (unreadCounts.get(selected.propertyId) ?? 0) : 0;

  const columns = useMemo<DataTableColumn<Lead>[]>(
    () => [
      {
        key: 'name',
        header: 'Lead',
        render: (row) => (
          <button
            type="button"
            onClick={() => markRead(row.id)}
            className="block text-left outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta"
          >
            <span className="flex items-center gap-2 font-semibold text-ink">
              {!isRead(row.id) && (
                <>
                  <span className="size-2 shrink-0 rounded-full bg-cta" aria-hidden="true" />
                  <span className="sr-only">Unread. </span>
                </>
              )}
              {row.name}
            </span>
            <span className="block text-xs text-ink-3">
              {new Date(row.createdAt).toLocaleString()}
            </span>
          </button>
        ),
      },
      {
        key: 'message',
        header: 'Message',
        render: (row) => (
          <span className="line-clamp-2 max-w-md text-ink-2" title={row.message}>
            {row.message}
          </span>
        ),
      },
      {
        key: 'contact',
        header: 'Contact',
        align: 'right',
        render: (row) => (
          <div className="flex items-center justify-end gap-1.5">
            <a
              href={`mailto:${row.email}`}
              aria-label={`Email ${row.name}`}
              className="grid size-8 place-items-center rounded-lg border border-line text-ink-2 transition-colors hover:bg-surface-3 hover:text-ink focus-visible:outline-2 focus-visible:outline-cta"
            >
              <Mail className="size-4" />
            </a>
            {row.phone && (
              <a
                href={`tel:${row.phone}`}
                aria-label={`Call ${row.name}`}
                className="grid size-8 place-items-center rounded-lg border border-line text-ink-2 transition-colors hover:bg-surface-3 hover:text-ink focus-visible:outline-2 focus-visible:outline-cta"
              >
                <Phone className="size-4" />
              </a>
            )}
          </div>
        ),
      },
    ],
    [isRead, markRead],
  );

  const totalLeads = result?.totalCount ?? 0;

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-ink">Leads</h1>
          <p className="mt-1 text-sm text-ink-2">
            {totalLeads} lead{totalLeads === 1 ? '' : 's'} across {groups.length}{' '}
            {groups.length === 1 ? 'listing' : 'listings'}
          </p>
        </div>

        <Input
          type="search"
          placeholder="Search name, email, message…"
          aria-label="Search leads"
          value={searchDraft}
          onChange={(e) => setSearchDraft(e.target.value)}
          className="sm:max-w-xs"
        />
      </div>

      {error && <Notice className="mb-4">{errorMessage(error, 'Could not load your leads')}</Notice>}

      {isPending ? (
        <div className="lg:grid lg:grid-cols-[280px_minmax(0,1fr)] lg:gap-6">
          <div className="mb-4 space-y-2 lg:mb-0" aria-hidden="true">
            {Array.from({ length: 4 }, (_, i) => (
              <Skeleton key={i} className="h-14 rounded-xl" />
            ))}
          </div>
          <div className="space-y-2" aria-hidden="true">
            {Array.from({ length: 4 }, (_, i) => (
              <Skeleton key={i} className="h-16 rounded-xl" />
            ))}
          </div>
        </div>
      ) : groups.length === 0 ? (
        <div className="flex flex-col items-center rounded-2xl border border-dashed border-line-strong bg-surface px-6 py-16 text-center">
          <Inbox className="size-7 text-ink-3" />
          <h2 className="mt-3 text-base font-semibold text-ink">
            {search ? 'No leads match your search' : 'No inquiries yet'}
          </h2>
          <p className="mt-1 max-w-sm text-sm text-ink-2">
            {search
              ? 'Try a different name, email or word from the message.'
              : 'When someone contacts you about a listing, it shows up here.'}
          </p>
        </div>
      ) : (
        <div className="lg:grid lg:grid-cols-[280px_minmax(0,1fr)] lg:gap-6">
          <PropertyList
            groups={groups}
            unreadCounts={unreadCounts}
            coverImages={coverImages}
            selectedId={selected?.propertyId}
            onSelect={selectProperty}
          />

          {selected && (
            <section className="mt-4 min-w-0 lg:mt-0">
              <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
                <div className="min-w-0">
                  <Link
                    to={paths.properties.detail(selected.propertyId)}
                    className="font-semibold text-ink underline-offset-4 hover:underline"
                  >
                    {selected.propertyTitle}
                  </Link>
                  <p className="text-sm text-ink-2">
                    {selected.propertyTownship} · {selected.leads.length}{' '}
                    {selected.leads.length === 1 ? 'lead' : 'leads'}
                    {selectedUnread > 0 ? ` · ${selectedUnread} unread` : ''}
                  </p>
                </div>
                {selectedUnread > 0 && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => markAllRead(selectedLeads.map((l) => l.id))}
                  >
                    <Check className="size-3.5" />
                    Mark all read
                  </Button>
                )}
              </div>

              <DataTable
                columns={columns}
                rows={selectedLeads}
                rowKey={(row) => row.id}
                emptyMessage="No inquiries yet for this listing."
              />
            </section>
          )}
        </div>
      )}
    </div>
  );
}

function PropertyList({
  groups,
  unreadCounts,
  coverImages,
  selectedId,
  onSelect,
}: {
  groups: PropertyGroup[];
  unreadCounts: Map<string, number>;
  coverImages: Map<string, string | null>;
  selectedId: string | undefined;
  onSelect: (propertyId: string) => void;
}) {
  return (
    <nav aria-label="Listings with leads" className="lg:border-r lg:border-line lg:pr-4">
      {/* Horizontal scroller on mobile, vertical list from lg — same pattern as the portal sidebar. */}
      <ul className="flex gap-2 overflow-x-auto pb-2 lg:flex-col lg:gap-1 lg:overflow-visible lg:pb-0">
        {groups.map((group) => {
          const unread = unreadCounts.get(group.propertyId) ?? 0;
          const active = group.propertyId === selectedId;
          const cover = coverImages.get(group.propertyId);

          return (
            <li key={group.propertyId} className="shrink-0 lg:shrink">
              <button
                type="button"
                aria-current={active ? 'true' : undefined}
                onClick={() => onSelect(group.propertyId)}
                className={cn(
                  'flex w-full min-w-[14rem] items-center gap-2.5 rounded-lg px-2.5 py-2 text-left outline-none lg:min-w-0',
                  'transition-colors duration-200 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-cta',
                  active ? 'bg-brand text-brand-fg' : 'text-ink-2 hover:bg-surface-3 hover:text-ink',
                )}
              >
                <span
                  className={cn(
                    'grid size-10 shrink-0 place-items-center overflow-hidden rounded-lg bg-surface-3',
                    active && 'bg-brand-fg/15',
                  )}
                >
                  {cover ? (
                    <img
                      src={cover}
                      alt=""
                      loading="lazy"
                      className="size-full object-cover"
                    />
                  ) : (
                    <Home className={cn('size-4', active ? 'text-brand-fg/70' : 'text-ink-3')} />
                  )}
                </span>

                <span className="min-w-0 flex-1">
                  <span className="block truncate text-sm font-medium">{group.propertyTitle}</span>
                  <span
                    className={cn(
                      'block truncate text-xs',
                      active ? 'text-brand-fg/70' : 'text-ink-3',
                    )}
                  >
                    {group.propertyTownship} · {group.leads.length}{' '}
                    {group.leads.length === 1 ? 'lead' : 'leads'}
                  </span>
                </span>
                {unread > 0 && (
                  <span
                    className={cn(
                      'tnum grid size-5 shrink-0 place-items-center rounded-full text-2xs font-bold',
                      active ? 'bg-brand-fg/20 text-brand-fg' : 'bg-cta text-cta-fg',
                    )}
                  >
                    {unread}
                    <span className="sr-only"> unread</span>
                  </span>
                )}
              </button>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
