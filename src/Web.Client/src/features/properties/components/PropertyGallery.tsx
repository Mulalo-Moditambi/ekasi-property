import { useCallback, useEffect, useState } from 'react';
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { ChevronLeft, ChevronRight, Expand, Home, X } from 'lucide-react';
import { Button } from '../../../shared/ui/button';
import { cn } from '../../../shared/ui/utils';

/**
 * Asymmetric media grid: one dominant frame with a 2×2 of thumbnails beside it,
 * collapsing to a single frame on small screens. Any tile opens the lightbox at
 * that photo. Degrades cleanly for listings with fewer than five photos.
 */
export function PropertyGallery({ urls, alt }: { urls: string[]; alt: string }) {
  const [lightboxAt, setLightboxAt] = useState<number | null>(null);

  if (urls.length === 0) {
    return (
      <div className="flex aspect-[16/9] w-full items-center justify-center rounded-2xl border border-line bg-surface-3 text-ink-3">
        <div className="flex flex-col items-center gap-2">
          <Home className="size-7" />
          <p className="text-sm">No photos yet</p>
        </div>
      </div>
    );
  }

  const [hero, ...rest] = urls;
  const thumbs = rest.slice(0, 4);

  return (
    <>
      <div className="relative overflow-hidden rounded-2xl border border-line">
        <div className={cn('grid gap-1.5', thumbs.length > 0 && 'sm:grid-cols-[1.6fr_1fr]')}>
          <button
            type="button"
            onClick={() => setLightboxAt(0)}
            aria-label={`Open photo 1 of ${urls.length}`}
            className="group relative aspect-[4/3] overflow-hidden bg-surface-3 outline-none focus-visible:outline-2 focus-visible:outline-offset-[-3px] focus-visible:outline-cta sm:aspect-auto sm:h-[26rem]"
          >
            <img
              src={hero}
              alt={`${alt} — main photo`}
              className="size-full object-cover transition-transform duration-300 ease-out group-hover:scale-[1.03]"
            />
          </button>

          {thumbs.length > 0 && (
            // With 1–2 extra photos a single column reads better than a 2×2 with holes in it.
            <div
              className={cn(
                'hidden gap-1.5 sm:grid',
                thumbs.length > 2 ? 'grid-cols-2 grid-rows-2' : 'grid-cols-1',
                thumbs.length === 2 && 'grid-rows-2',
              )}
            >
              {thumbs.map((url, index) => (
                <button
                  key={url}
                  type="button"
                  onClick={() => setLightboxAt(index + 1)}
                  aria-label={`Open photo ${index + 2} of ${urls.length}`}
                  className="group relative overflow-hidden bg-surface-3 outline-none focus-visible:outline-2 focus-visible:outline-offset-[-3px] focus-visible:outline-cta"
                >
                  <img
                    src={url}
                    alt={`${alt} — photo ${index + 2}`}
                    loading="lazy"
                    className="size-full object-cover transition-transform duration-300 ease-out group-hover:scale-[1.03]"
                  />
                </button>
              ))}
            </div>
          )}
        </div>

        <Button
          variant="outline"
          size="sm"
          onClick={() => setLightboxAt(0)}
          className="absolute bottom-3 right-3 bg-surface/95 shadow-md backdrop-blur"
        >
          <Expand />
          View all photos ({urls.length})
        </Button>
      </div>

      {lightboxAt !== null && (
        <Lightbox urls={urls} alt={alt} startAt={lightboxAt} onClose={() => setLightboxAt(null)} />
      )}
    </>
  );
}

function Lightbox({
  urls,
  alt,
  startAt,
  onClose,
}: {
  urls: string[];
  alt: string;
  startAt: number;
  onClose: () => void;
}) {
  const [index, setIndex] = useState(startAt);

  const step = useCallback(
    (direction: -1 | 1) => setIndex((current) => (current + direction + urls.length) % urls.length),
    [urls.length],
  );

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (event.key === 'ArrowLeft') step(-1);
      if (event.key === 'ArrowRight') step(1);
    }

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [step]);

  return (
    <DialogPrimitive.Root open onOpenChange={(open) => !open && onClose()}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-slate-950/90" />
        <DialogPrimitive.Content className="fixed inset-0 z-50 flex flex-col outline-none">
          <DialogPrimitive.Title className="sr-only">{alt} — photo viewer</DialogPrimitive.Title>

          <div className="flex items-center justify-between px-4 py-3 text-white">
            <span className="tnum text-sm">
              {index + 1} / {urls.length}
            </span>
            <DialogPrimitive.Close
              aria-label="Close"
              className="rounded-lg p-2 transition-colors hover:bg-white/10 focus-visible:outline-2 focus-visible:outline-white"
            >
              <X className="size-5" />
            </DialogPrimitive.Close>
          </div>

          <div className="relative flex min-h-0 flex-1 items-center justify-center px-4 pb-6">
            <img
              src={urls[index]}
              alt={`${alt} — photo ${index + 1}`}
              className="max-h-full max-w-full rounded-lg object-contain"
            />

            {urls.length > 1 && (
              <>
                <LightboxNav side="left" onClick={() => step(-1)} />
                <LightboxNav side="right" onClick={() => step(1)} />
              </>
            )}
          </div>
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  );
}

function LightboxNav({ side, onClick }: { side: 'left' | 'right'; onClick: () => void }) {
  const Icon = side === 'left' ? ChevronLeft : ChevronRight;

  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={side === 'left' ? 'Previous photo' : 'Next photo'}
      className={cn(
        // Anchored to the viewport, not the photo, so the arrows never shift between images.
        'fixed top-1/2 z-10 grid size-11 -translate-y-1/2 place-items-center rounded-full',
        'bg-white/10 text-white backdrop-blur transition-colors hover:bg-white/20',
        'focus-visible:outline-2 focus-visible:outline-white',
        side === 'left' ? 'left-4' : 'right-4',
      )}
    >
      <Icon className="size-5" />
    </button>
  );
}
