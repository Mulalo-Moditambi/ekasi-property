import { ListingType } from '../features/properties/types';

const zar = new Intl.NumberFormat('en-ZA', {
  style: 'currency',
  currency: 'ZAR',
  maximumFractionDigits: 0,
});

/**
 * Price split into amount and period so a dense row can set them at different
 * sizes and still keep the numbers aligned in their column.
 */
export function priceParts(price: number, listingType: ListingType): {
  amount: string;
  period: string | null;
} {
  return {
    amount: zar.format(price),
    period: listingType === ListingType.Rent ? 'per month' : null,
  };
}

const RELATIVE_UNITS: [limitInSeconds: number, secondsPerUnit: number, unit: Intl.RelativeTimeFormatUnit][] = [
  [60, 1, 'second'],
  [3600, 60, 'minute'],
  [86400, 3600, 'hour'],
  [2592000, 86400, 'day'],
  [31536000, 2592000, 'month'],
  [Infinity, 31536000, 'year'],
];

const relative = new Intl.RelativeTimeFormat('en-ZA', { numeric: 'auto' });

/** "3 days ago" — used for listing freshness, which is a real ranking signal for renters. */
export function relativeTime(iso: string): string {
  const elapsedSeconds = (Date.parse(iso) - Date.now()) / 1000;

  if (!Number.isFinite(elapsedSeconds)) {
    return '';
  }

  const magnitude = Math.abs(elapsedSeconds);
  const [, secondsPerUnit, unit] = RELATIVE_UNITS.find(([limit]) => magnitude < limit)!;

  return relative.format(Math.round(elapsedSeconds / secondsPerUnit), unit);
}

/** Listings newer than this get a "New" flag in the results. */
export function isRecent(iso: string, withinDays = 7): boolean {
  return Date.now() - Date.parse(iso) < withinDays * 86400_000;
}
