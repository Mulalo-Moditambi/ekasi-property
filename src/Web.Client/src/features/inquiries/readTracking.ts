// Lightweight client-side read/unread tracking for leads — there is no
// backend status field for inquiries, so "read" is purely a local UX layer.
const KEY = 'ekasi.readLeads';

function getReadSet(): Set<string> {
  try {
    const raw = localStorage.getItem(KEY);
    return new Set(raw ? (JSON.parse(raw) as string[]) : []);
  } catch {
    return new Set();
  }
}

function saveReadSet(set: Set<string>): void {
  localStorage.setItem(KEY, JSON.stringify([...set]));
}

export function isLeadRead(id: string): boolean {
  return getReadSet().has(id);
}

export function markLeadRead(id: string): void {
  const set = getReadSet();
  set.add(id);
  saveReadSet(set);
}

export function markLeadsRead(ids: string[]): void {
  const set = getReadSet();
  for (const id of ids) set.add(id);
  saveReadSet(set);
}

export function countUnread(ids: string[]): number {
  const set = getReadSet();
  return ids.filter((id) => !set.has(id)).length;
}
