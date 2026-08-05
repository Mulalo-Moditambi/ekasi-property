import type { ReactNode } from 'react';

export function FormSection({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="rounded-2xl border border-line bg-surface p-5 shadow-sm">
      <h2 className="mb-4 text-base font-semibold tracking-tight text-ink">{title}</h2>
      <div className="space-y-4">{children}</div>
    </section>
  );
}

/** Every step receives the same shape, so they stay interchangeable and reorderable. */
export interface StepProps {
  disabled?: boolean;
}
