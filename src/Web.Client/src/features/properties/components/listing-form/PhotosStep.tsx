import { useRef } from 'react';
import type { ChangeEvent, DragEvent } from 'react';
import { ChevronLeft, ChevronRight, GripVertical, ImagePlus, X } from 'lucide-react';
import { cn } from '../../../../shared/ui/utils';
import { FormSection } from './FormSection';
import type { PhotoDraft } from '../../usePhotoDrafts';
import type { PropertyImage } from '../../types';

export const MAX_PHOTOS = 10;

interface PhotosStepProps {
  /** Already uploaded, and therefore reorderable and individually deletable. */
  existing: PropertyImage[];
  drafts: PhotoDraft[];
  onSelectFiles: (files: File[]) => void;
  onRemoveDraft: (id: string) => void;
  onRemoveExisting: (image: PropertyImage) => void;
  /** Moves the image at `from` to `to`. Persisted optimistically by the caller. */
  onReorder: (from: number, to: number) => void;
}

export function PhotosStep({
  existing,
  drafts,
  onSelectFiles,
  onRemoveDraft,
  onRemoveExisting,
  onReorder,
}: PhotosStepProps) {
  const dragIndex = useRef<number | null>(null);
  const photoCount = existing.length + drafts.length;
  const isFull = photoCount >= MAX_PHOTOS;

  function handleFiles(event: ChangeEvent<HTMLInputElement>) {
    onSelectFiles(Array.from(event.target.files ?? []));
    // Reset so re-picking the same file still fires a change event.
    event.target.value = '';
  }

  function handleDrop(index: number) {
    const from = dragIndex.current;
    dragIndex.current = null;

    if (from !== null && from !== index) {
      onReorder(from, index);
    }
  }

  return (
    <FormSection title="Photos">
      <label
        className={cn(
          'flex cursor-pointer flex-col items-center gap-1.5 rounded-xl border border-dashed border-line-strong',
          'bg-surface-2 px-4 py-6 text-center transition-colors hover:border-cta hover:bg-cta-soft/40',
          isFull && 'pointer-events-none opacity-50',
        )}
      >
        <ImagePlus className="size-5 text-ink-3" />
        <span className="text-sm font-medium text-ink">Add photos</span>
        <span className="text-xs text-ink-3">
          Up to {MAX_PHOTOS} · JPEG, PNG or WebP · max 5&nbsp;MB each ({photoCount}/{MAX_PHOTOS} used)
        </span>
        <input
          type="file"
          multiple
          accept="image/jpeg,image/png,image/webp"
          onChange={handleFiles}
          disabled={isFull}
          className="sr-only"
        />
      </label>

      {photoCount > 0 && (
        <ul className="grid grid-cols-3 gap-2 sm:grid-cols-4">
          {existing.map((image, index) => (
            <li
              key={image.id}
              draggable
              onDragStart={() => {
                dragIndex.current = index;
              }}
              onDragOver={(event: DragEvent) => event.preventDefault()}
              onDrop={() => handleDrop(index)}
              className={cn(
                'group relative aspect-square overflow-hidden rounded-xl border bg-surface-3',
                index === 0 ? 'border-cta' : 'border-line',
              )}
            >
              <img src={image.url} alt="" loading="lazy" className="size-full object-cover" />

              <span className="absolute left-1.5 top-1.5 grid size-5 place-items-center rounded bg-black/50 text-white">
                <GripVertical className="size-3" aria-hidden="true" />
              </span>

              {index === 0 && (
                <span className="absolute bottom-1.5 left-1.5 rounded bg-cta px-1.5 py-0.5 text-2xs font-semibold text-cta-fg">
                  Cover
                </span>
              )}

              {/*
                Drag-and-drop is mouse-only, which made reordering — and therefore
                choosing a cover photo — impossible from the keyboard. These
                buttons give the same capability to every input method.
              */}
              {existing.length > 1 && (
                <span className="absolute inset-x-1.5 bottom-1.5 flex justify-end gap-1 opacity-0 transition-opacity focus-within:opacity-100 group-hover:opacity-100">
                  <MoveButton
                    label={`Move photo ${index + 1} earlier`}
                    disabled={index === 0}
                    onClick={() => onReorder(index, index - 1)}
                  >
                    <ChevronLeft className="size-3.5" />
                  </MoveButton>
                  <MoveButton
                    label={`Move photo ${index + 1} later`}
                    disabled={index === existing.length - 1}
                    onClick={() => onReorder(index, index + 1)}
                  >
                    <ChevronRight className="size-3.5" />
                  </MoveButton>
                </span>
              )}

              <RemovePhoto label={`Remove photo ${index + 1}`} onClick={() => onRemoveExisting(image)} />
            </li>
          ))}

          {drafts.map((draft) => (
            <li
              key={draft.id}
              className="relative aspect-square overflow-hidden rounded-xl border border-line bg-surface-3"
            >
              <img src={draft.previewUrl} alt={draft.file.name} className="size-full object-cover" />
              <span className="absolute bottom-1.5 left-1.5 rounded bg-black/55 px-1.5 py-0.5 text-2xs font-medium text-white">
                Pending
              </span>
              <RemovePhoto label={`Remove ${draft.file.name}`} onClick={() => onRemoveDraft(draft.id)} />
            </li>
          ))}
        </ul>
      )}

      {existing.length > 1 && (
        <p className="text-xs text-ink-3">
          Drag photos to reorder, or use the arrows — the first is the cover photo.
        </p>
      )}
    </FormSection>
  );
}

function MoveButton({
  label,
  disabled,
  onClick,
  children,
}: {
  label: string;
  disabled: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className="grid size-6 place-items-center rounded bg-black/60 text-white outline-none transition-colors hover:bg-black/80 focus-visible:opacity-100 focus-visible:outline-2 focus-visible:outline-white disabled:opacity-30"
    >
      {children}
    </button>
  );
}

function RemovePhoto({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <button
      type="button"
      aria-label={label}
      onClick={onClick}
      className="absolute right-1.5 top-1.5 grid size-6 place-items-center rounded-full bg-black/55 text-white outline-none transition-colors hover:bg-danger focus-visible:outline-2 focus-visible:outline-white"
    >
      <X className="size-3.5" />
    </button>
  );
}
