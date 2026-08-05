import { NavLink } from 'react-router-dom';
import { cn } from '../../../../shared/ui/utils';
import { paths } from '../../../../app/routes/paths';

const TABS = [
  { label: 'Overview', to: paths.properties.overview },
  { label: 'Amenities', to: paths.properties.amenities },
  { label: 'Location', to: paths.properties.location },
] as const;

/**
 * Tabs are links, not buttons: each panel has its own URL, so a specific tab is
 * shareable and the Back button steps between them. `NavLink`'s `aria-current`
 * carries the selected state for assistive tech; there is no `role="tablist"`
 * here because these are navigations, not in-page tab panels, and announcing
 * them as a tablist would promise keyboard behaviour links don't have.
 */
export function DetailTabs({ propertyId }: { propertyId: string }) {
  return (
    <nav aria-label="Listing sections" className="mt-6 border-b border-line">
      <ul className="-mb-px flex gap-1">
        {TABS.map(({ label, to }) => (
          <li key={label}>
            <NavLink
              to={to(propertyId)}
              className={({ isActive }) =>
                cn(
                  'inline-block border-b-2 px-3.5 py-2.5 text-sm font-medium transition-colors',
                  'outline-none focus-visible:outline-2 focus-visible:outline-offset-[-2px] focus-visible:outline-cta',
                  isActive
                    ? 'border-cta text-ink'
                    : 'border-transparent text-ink-2 hover:border-line-strong hover:text-ink',
                )
              }
            >
              {label}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}
