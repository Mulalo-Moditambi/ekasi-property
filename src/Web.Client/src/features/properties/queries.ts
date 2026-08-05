import {
  keepPreviousData,
  useMutation,
  useQueries,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query';
import type { QueryClient } from '@tanstack/react-query';
import {
  createListing,
  deleteImage,
  deleteListing,
  getMyProperties,
  getProperty,
  markRented,
  markSold,
  relistProperty,
  reorderImages,
  searchProperties,
  updateListing,
  uploadImages,
  withdrawListing,
} from './api';
import { PropertyStatus } from './types';
import type {
  CreateListingRequest,
  MyPropertiesFilters,
  PropertyDetail,
  PropertySort,
  SearchFilters,
} from './types';

/**
 * One factory for every properties cache key. Keys are hierarchical, so a
 * mutation can invalidate a whole branch (`propertyKeys.lists()`) without
 * knowing which filter combinations happen to be cached.
 */
export const propertyKeys = {
  all: ['properties'] as const,
  lists: () => [...propertyKeys.all, 'list'] as const,
  list: (filters: SearchFilters, page: number, pageSize: number, sort: PropertySort) =>
    [...propertyKeys.lists(), { filters, page, pageSize, sort }] as const,
  mine: () => [...propertyKeys.all, 'mine'] as const,
  myList: (filters: MyPropertiesFilters, page: number, pageSize: number) =>
    [...propertyKeys.mine(), { filters, page, pageSize }] as const,
  details: () => [...propertyKeys.all, 'detail'] as const,
  detail: (id: string) => [...propertyKeys.details(), id] as const,
};

interface SearchArgs {
  filters: SearchFilters;
  page: number;
  pageSize: number;
  sort: PropertySort;
}

export function useSearchProperties({ filters, page, pageSize, sort }: SearchArgs) {
  return useQuery({
    queryKey: propertyKeys.list(filters, page, pageSize, sort),
    queryFn: () => searchProperties(filters, page, pageSize, sort),
    // Paging and filtering keep the previous page on screen instead of dropping
    // back to skeletons, so the results column doesn't collapse and rebuild.
    placeholderData: keepPreviousData,
  });
}

export function useProperty(id: string | undefined) {
  return useQuery({
    queryKey: propertyKeys.detail(id ?? ''),
    queryFn: () => getProperty(id as string),
    enabled: Boolean(id),
  });
}

export function useMyProperties(filters: MyPropertiesFilters, page = 1, pageSize = 50) {
  return useQuery({
    queryKey: propertyKeys.myList(filters, page, pageSize),
    queryFn: () => getMyProperties(filters, page, pageSize),
    placeholderData: keepPreviousData,
  });
}

const COUNTED_STATUSES = [
  PropertyStatus.Listed,
  PropertyStatus.Rented,
  PropertyStatus.Sold,
  PropertyStatus.Withdrawn,
] as const;

/**
 * Per-status totals for the overview. The API has no aggregate endpoint, so
 * each status is a `pageSize: 1` query read for its `totalCount`. Running them
 * through `useQueries` means each one is cached and invalidated on its own —
 * and the four share the cache with anything else already asking the same
 * question, so the tiles are usually instant on a return visit.
 */
export function useMyPropertyStatusCounts() {
  const results = useQueries({
    queries: COUNTED_STATUSES.map((status) => ({
      queryKey: propertyKeys.myList({ status }, 1, 1),
      queryFn: () => getMyProperties({ status }, 1, 1),
    })),
  });

  const counts = {} as Record<PropertyStatus, number>;
  COUNTED_STATUSES.forEach((status, index) => {
    counts[status] = results[index].data?.totalCount ?? 0;
  });

  return {
    counts,
    total: COUNTED_STATUSES.reduce((sum, status) => sum + counts[status], 0),
    isPending: results.some((result) => result.isPending),
    error: results.find((result) => result.error)?.error ?? null,
  };
}

/**
 * Warms the detail cache from a listing card's hover/focus. The `staleTime`
 * guard means repeatedly sweeping the grid costs at most one request per
 * listing, and by the time the click lands the page usually renders from cache.
 */
export function prefetchProperty(client: QueryClient, id: string): void {
  void client.prefetchQuery({
    queryKey: propertyKeys.detail(id),
    queryFn: () => getProperty(id),
    staleTime: 30_000,
  });
}

/** Any write to a listing can change both the browse lists and the owner's table. */
function useInvalidateProperties() {
  const client = useQueryClient();

  return (id?: string) => {
    void client.invalidateQueries({ queryKey: propertyKeys.lists() });
    void client.invalidateQueries({ queryKey: propertyKeys.mine() });
    if (id) {
      void client.invalidateQueries({ queryKey: propertyKeys.detail(id) });
    }
  };
}

export function useCreateListing() {
  const invalidate = useInvalidateProperties();

  return useMutation({
    mutationFn: (body: CreateListingRequest) => createListing(body),
    onSuccess: (id) => invalidate(id),
  });
}

type UpdateListingBody = Omit<CreateListingRequest, 'ownerId' | 'listingType' | 'propertyType'>;

export function useUpdateListing() {
  const invalidate = useInvalidateProperties();

  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: UpdateListingBody }) => updateListing(id, body),
    onSuccess: (_result, { id }) => invalidate(id),
  });
}

export function useUploadImages() {
  const invalidate = useInvalidateProperties();

  return useMutation({
    mutationFn: ({ id, files }: { id: string; files: File[] }) => uploadImages(id, files),
    onSuccess: (_result, { id }) => invalidate(id),
  });
}

export function useDeleteImage() {
  const invalidate = useInvalidateProperties();

  return useMutation({
    mutationFn: ({ id, imageId }: { id: string; imageId: string }) => deleteImage(id, imageId),
    onSuccess: (_result, { id }) => invalidate(id),
  });
}

/**
 * Drag-to-reorder is applied to the cache immediately and rolled back verbatim
 * if the API rejects it — previously a failed reorder left the UI showing an
 * order the server had refused.
 */
export function useReorderImages() {
  const client = useQueryClient();

  return useMutation({
    mutationFn: ({ id, imageIds }: { id: string; imageIds: string[] }) => reorderImages(id, imageIds),
    onMutate: async ({ id, imageIds }) => {
      await client.cancelQueries({ queryKey: propertyKeys.detail(id) });
      const previous = client.getQueryData<PropertyDetail>(propertyKeys.detail(id));

      if (previous) {
        const byId = new Map(previous.images.map((image) => [image.id, image]));
        const images = imageIds.flatMap((imageId) => {
          const image = byId.get(imageId);
          return image ? [image] : [];
        });

        client.setQueryData<PropertyDetail>(propertyKeys.detail(id), { ...previous, images });
      }

      return { previous };
    },
    onError: (_error, { id }, context) => {
      if (context?.previous) {
        client.setQueryData(propertyKeys.detail(id), context.previous);
      }
    },
    onSettled: (_result, _error, { id }) => {
      void client.invalidateQueries({ queryKey: propertyKeys.detail(id) });
      void client.invalidateQueries({ queryKey: propertyKeys.lists() });
    },
  });
}

const lifecycleActions = {
  markRented,
  markSold,
  withdraw: withdrawListing,
  relist: relistProperty,
  remove: deleteListing,
} as const;

export type LifecycleAction = keyof typeof lifecycleActions;

export const lifecycleMessages: Record<LifecycleAction, string> = {
  markRented: 'Marked as rented',
  markSold: 'Marked as sold',
  withdraw: 'Listing withdrawn',
  relist: 'Listing relisted',
  remove: 'Listing deleted',
};

/**
 * The five status transitions share a shape, so they share a mutation. `variables`
 * doubles as the "which row is busy" flag, which removes the old manual `busyId`
 * state from every page that offered these actions.
 */
export function useListingLifecycle() {
  const invalidate = useInvalidateProperties();

  return useMutation({
    mutationFn: ({ id, action }: { id: string; action: LifecycleAction }) => lifecycleActions[action](id),
    onSuccess: (_result, { id }) => invalidate(id),
  });
}
