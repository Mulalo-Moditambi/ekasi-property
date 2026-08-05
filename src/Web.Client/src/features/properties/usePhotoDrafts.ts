import { useCallback, useEffect, useMemo, useRef, useState } from 'react';

export interface PhotoDraft {
  /** Stable identity for React keys and removal — file name + size can collide. */
  id: string;
  file: File;
  /** Object URL for the local preview, revoked when the draft goes away. */
  previewUrl: string;
}

let sequence = 0;

/**
 * Pending (not yet uploaded) photos, with their preview URLs owned properly.
 *
 * The previous implementation called `URL.createObjectURL(file)` inline in JSX,
 * which minted a fresh blob URL on *every render* — one per pending photo per
 * keystroke elsewhere in the form — and never revoked any of them. Each one
 * pins its File in memory until the tab closes. Here a URL is created once when
 * a file is added and revoked when it is removed or the form unmounts.
 */
export function usePhotoDrafts(maxTotal: number, usedSlots: number) {
  const [drafts, setDrafts] = useState<PhotoDraft[]>([]);

  const add = useCallback(
    (files: File[]) => {
      setDrafts((current) => {
        const room = maxTotal - usedSlots - current.length;
        if (room <= 0) {
          return current;
        }

        const added = files.slice(0, room).map((file) => ({
          id: `draft-${(sequence += 1)}`,
          file,
          previewUrl: URL.createObjectURL(file),
        }));

        return [...current, ...added];
      });
    },
    [maxTotal, usedSlots],
  );

  const remove = useCallback((id: string) => {
    setDrafts((current) => {
      const target = current.find((draft) => draft.id === id);
      if (target) {
        URL.revokeObjectURL(target.previewUrl);
      }

      return current.filter((draft) => draft.id !== id);
    });
  }, []);

  const clear = useCallback(() => {
    setDrafts((current) => {
      for (const draft of current) {
        URL.revokeObjectURL(draft.previewUrl);
      }

      return [];
    });
  }, []);

  // Tracked in a ref so the unmount cleanup below doesn't need `drafts` as a
  // dependency — that would revoke live URLs on every change, not at teardown.
  const draftsRef = useRef(drafts);
  draftsRef.current = drafts;

  // Last line of defence: whatever is still pending when the form closes.
  useEffect(
    () => () => {
      for (const draft of draftsRef.current) {
        URL.revokeObjectURL(draft.previewUrl);
      }
    },
    [],
  );

  const files = useMemo(() => drafts.map((draft) => draft.file), [drafts]);

  return { drafts, files, add, remove, clear } as const;
}
