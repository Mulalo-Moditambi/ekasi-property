import type { ReactNode } from 'react';
import { Check } from 'lucide-react';
import { LogoMark } from '../components/Logo';

const PROMISES = [
  'Deal directly with owners — no agent commission',
  'Backrooms, cottages and houses across the townships',
  'Free to list, free to browse',
];

/**
 * Split-screen auth shell: the form on the left, a navy brand panel on the right
 * that collapses away below `lg` so small screens get the form immediately.
 */
export function AuthLayout({
  title,
  subtitle,
  children,
  footer,
}: {
  title: string;
  subtitle: ReactNode;
  children: ReactNode;
  footer?: ReactNode;
}) {
  return (
    <div className="grid min-h-[calc(100vh-3.5rem)] lg:grid-cols-2">
      <div className="flex items-center justify-center px-4 py-12 sm:px-8">
        <div className="w-full max-w-sm">
          <h1 className="text-2xl font-bold tracking-tight text-ink">{title}</h1>
          <p className="mt-1.5 text-sm text-ink-2">{subtitle}</p>

          <div className="mt-7">{children}</div>

          {footer && <div className="mt-6 text-sm text-ink-2">{footer}</div>}
        </div>
      </div>

      <aside className="relative hidden overflow-hidden bg-hero-gradient lg:block" aria-hidden="true">
        <div className="relative flex h-full flex-col justify-center px-12">
          <LogoMark className="size-11 rounded-xl bg-white/10 [&_svg]:size-6" />
          <p className="mt-7 max-w-md font-display text-3xl font-bold leading-tight tracking-tight text-white">
            Property, straight from the people who own it.
          </p>
          <ul className="mt-8 space-y-3.5">
            {PROMISES.map((promise) => (
              <li key={promise} className="flex items-start gap-3 text-[15px] text-white/75">
                <span className="mt-0.5 grid size-5 shrink-0 place-items-center rounded-full bg-cta/20">
                  <Check className="size-3 text-cta" strokeWidth={3} />
                </span>
                {promise}
              </li>
            ))}
          </ul>
        </div>
      </aside>
    </div>
  );
}
