import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

/**
 * A two-way mapping between the query string and a typed state object. Parsing
 * must be total: any hand-edited URL has to yield a usable state rather than
 * throwing, so `parse` is expected to fall back rather than validate.
 */
export interface UrlStateCodec<T> {
  parse: (params: URLSearchParams) => T;
  serialize: (state: T) => URLSearchParams;
}

export interface UrlStateUpdateOptions {
  /**
   * `replace` overwrites the current history entry instead of pushing a new one.
   * Use it for continuous input (typing, dragging a slider) so Back steps out of
   * the search rather than crawling back through every intermediate keystroke.
   */
  replace?: boolean;
}

/**
 * The URL is the state. Nothing is mirrored into React state, so there is no
 * second copy to fall out of sync, and Back/Forward/refresh/paste all work by
 * construction.
 */
export function useUrlState<T>(codec: UrlStateCodec<T>) {
  const [searchParams, setSearchParams] = useSearchParams();

  // `codec` is typically an object literal at the call site; keying the memo on
  // the serialised query string keeps the parsed value stable across renders.
  const query = searchParams.toString();
  const state = useMemo(() => codec.parse(new URLSearchParams(query)), [query, codec]);

  const setState = useCallback(
    (updater: T | ((current: T) => T), options: UrlStateUpdateOptions = {}) => {
      setSearchParams(
        (current) => {
          const parsed = codec.parse(current);
          const next = typeof updater === 'function' ? (updater as (c: T) => T)(parsed) : updater;
          return codec.serialize(next);
        },
        { replace: options.replace ?? false },
      );
    },
    [codec, setSearchParams],
  );

  return { state, query, setState } as const;
}

/**
 * Local-first text input that lands in the URL only once typing pauses.
 *
 * Without this, every keystroke is a history entry and a refetch. The local
 * value stays authoritative while the user types, and re-syncs when the URL
 * changes from somewhere else (Back button, a filter chip being cleared).
 */
export function useDebouncedInput(
  value: string,
  commit: (next: string) => void,
  delay = 300,
): readonly [string, (next: string) => void] {
  const [draft, setDraft] = useState(value);
  const commitRef = useRef(commit);
  commitRef.current = commit;

  // External changes win — the user is not mid-keystroke when the URL moves.
  useEffect(() => setDraft(value), [value]);

  const onChange = useCallback((next: string) => setDraft(next), []);

  useEffect(() => {
    if (draft === value) {
      return;
    }

    const timer = window.setTimeout(() => commitRef.current(draft), delay);
    return () => window.clearTimeout(timer);
  }, [draft, value, delay]);

  return [draft, onChange] as const;
}
