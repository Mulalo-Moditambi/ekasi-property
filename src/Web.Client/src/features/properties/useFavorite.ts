import { useCallback, useEffect, useState } from 'react';
import type { MouseEvent } from 'react';
import { isFavorite, toggleFavorite } from './favorites';

/**
 * Saved state for one listing. The toggle swallows the click so it can sit on
 * top of a card that is itself a link to the detail page.
 */
export function useFavorite(id: string) {
  const [saved, setSaved] = useState(() => isFavorite(id));

  useEffect(() => setSaved(isFavorite(id)), [id]);

  const toggle = useCallback(
    (event: MouseEvent) => {
      event.preventDefault();
      event.stopPropagation();
      setSaved(toggleFavorite(id));
    },
    [id],
  );

  return { saved, toggle };
}
