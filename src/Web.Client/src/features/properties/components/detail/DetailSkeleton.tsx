import { Skeleton } from '../../../../shared/ui/skeleton';

export function DetailSkeleton() {
  return (
    <div className="mx-auto max-w-[1400px] px-4 pb-16 pt-5 sm:px-6" aria-hidden="true">
      <Skeleton className="mb-4 h-4 w-52" />
      <Skeleton className="aspect-[4/3] w-full rounded-2xl sm:aspect-auto sm:h-[26rem]" />
      <div className="mt-6 lg:grid lg:grid-cols-[minmax(0,1fr)_22rem] lg:gap-8">
        <div className="space-y-3">
          <Skeleton className="h-5 w-40" />
          <Skeleton className="h-9 w-3/4" />
          <Skeleton className="h-4 w-2/3" />
          <Skeleton className="h-10 w-48" />
        </div>
        <Skeleton className="mt-8 h-80 w-full rounded-2xl lg:mt-0" />
      </div>
    </div>
  );
}
