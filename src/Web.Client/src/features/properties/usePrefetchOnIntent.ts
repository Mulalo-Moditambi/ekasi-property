import { useCallback, useMemo, useRef } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { prefetchProperty } from './queries';

/**
 * Warms a listing's detail cache the moment the user shows intent — hovering,
 * focusing, or starting a touch. By the time the click resolves the request is
 * usually already in flight or complete, which removes the skeleton flash on
 * the browse → detail step entirely.
 *
 * Spread the result onto the card's root element.
 *
 * Cheap by construction: `prefetchQuery` respects `staleTime`, so a cached
 * listing costs nothing, and the local ref stops a single hover from firing on
 * every pointer event.
 */
export function usePrefetchOnIntent(propertyId: string) {
  const client = useQueryClient();
  const requested = useRef(false);

  const warm = useCallback(() => {
    if (requested.current) {
      return;
    }

    requested.current = true;
    prefetchProperty(client, propertyId);
  }, [client, propertyId]);

  return useMemo(
    () => ({
      onMouseEnter: warm,
      onFocus: warm,
      onTouchStart: warm,
    }),
    [warm],
  );
}
