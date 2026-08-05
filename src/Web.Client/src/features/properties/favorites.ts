// Frontend-only saved listings — no backend persistence for this yet.
const KEY = 'ekasi.favorites';

function getSet(): Set<string> {
  try {
    const raw = localStorage.getItem(KEY);
    return new Set(raw ? (JSON.parse(raw) as string[]) : []);
  } catch {
    return new Set();
  }
}

export function isFavorite(id: string): boolean {
  return getSet().has(id);
}

export function toggleFavorite(id: string): boolean {
  const set = getSet();
  if (set.has(id)) {
    set.delete(id);
  } else {
    set.add(id);
  }
  localStorage.setItem(KEY, JSON.stringify([...set]));
  return set.has(id);
}
