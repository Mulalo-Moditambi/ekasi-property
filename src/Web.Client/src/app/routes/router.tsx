import { lazy } from 'react';
import type { ComponentType } from 'react';
import { Navigate, createBrowserRouter, useLocation, useParams } from 'react-router-dom';
import { AppShell } from '../../shared/layout/AppShell';
import { DashboardLayout } from '../../shared/layout/DashboardLayout';
import { BrowsePage } from '../../pages/BrowsePage';
import { PropertyDetailPage } from '../../pages/PropertyDetailPage';
import { OverviewTab } from '../../features/properties/components/detail/OverviewTab';
import { AmenitiesTab } from '../../features/properties/components/detail/AmenitiesTab';
import { LocationTab } from '../../features/properties/components/detail/LocationTab';
import { ProtectedRoute } from './ProtectedRoute';
import { ForbiddenPage, NotFoundPage, RouteErrorPage } from './StatusPage';
import { paths } from './paths';

/**
 * Route-level splitting. The browse and detail pages stay in the entry chunk —
 * they are the pages every visitor and every shared link lands on, so deferring
 * them would only add a round trip. Everything behind a login, plus the auth
 * forms themselves, is fetched on demand: an anonymous browser never downloads
 * the owner portal or the listing wizard.
 */
/** `lazy()` wants a default export; our pages are named. This adapts them. */
const named =
  <T extends string>(name: T) =>
  (module: Record<T, ComponentType>) => ({ default: module[name] });

const LoginPage = lazy(() => import('../../pages/LoginPage').then(named('LoginPage')));
const RegisterPage = lazy(() => import('../../pages/RegisterPage').then(named('RegisterPage')));
const DashboardOverviewPage = lazy(() =>
  import('../../pages/dashboard/DashboardOverviewPage').then(named('DashboardOverviewPage')),
);
const ManageListingsPage = lazy(() =>
  import('../../pages/dashboard/ManageListingsPage').then(named('ManageListingsPage')),
);
const ManageLeadsPage = lazy(() =>
  import('../../pages/dashboard/ManageLeadsPage').then(named('ManageLeadsPage')),
);
const ListingFormPage = lazy(() =>
  import('../../pages/dashboard/ListingFormPage').then(named('ListingFormPage')),
);

/**
 * Keeps links shared before the `/dashboard` → `/manager` rename working, and
 * preserves the query string so a filtered view survives the hop.
 */
function LegacyDashboardRedirect({ to }: { to: string }) {
  const { search, hash } = useLocation();
  return <Navigate to={`${to}${search}${hash}`} replace />;
}

function LegacyEditRedirect() {
  const { propertyId } = useParams<{ propertyId: string }>();
  return <Navigate to={paths.manager.editProperty(propertyId ?? '')} replace />;
}

export const router = createBrowserRouter([
  {
    element: <AppShell />,
    errorElement: <RouteErrorPage />,
    children: [
      // `/` is a marketing-free app, so the front door *is* the marketplace.
      { index: true, element: <Navigate to={paths.properties.root} replace /> },

      { path: 'properties', element: <BrowsePage /> },

      {
        path: 'properties/:propertyId',
        element: <PropertyDetailPage />,
        children: [
          { index: true, element: <Navigate to="overview" replace /> },
          { path: 'overview', element: <OverviewTab /> },
          { path: 'amenities', element: <AmenitiesTab /> },
          { path: 'location', element: <LocationTab /> },
        ],
      },

      // AuthLayout is a prop-driven shell the pages render themselves, so these
      // sit inside AppShell and keep the site header and footer.
      { path: 'login', element: <LoginPage /> },
      { path: 'register', element: <RegisterPage /> },

      { path: '403', element: <ForbiddenPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },

  {
    path: 'manager',
    errorElement: <RouteErrorPage />,
    // Guard first, layout inside: an unauthenticated visitor is redirected
    // before the sidebar and its queries ever mount.
    element: <ProtectedRoute roles={['owner']} />,
    children: [
      {
        element: <DashboardLayout />,
        children: [
          { index: true, element: <Navigate to={paths.manager.dashboard} replace /> },
          { path: 'dashboard', element: <DashboardOverviewPage /> },
          { path: 'properties', element: <ManageListingsPage /> },
          { path: 'properties/new', element: <ListingFormPage /> },
          { path: 'properties/:propertyId/edit', element: <ListingFormPage /> },
          { path: 'leads', element: <ManageLeadsPage /> },
          { path: '*', element: <NotFoundPage /> },
        ],
      },
    ],
  },

  // Pre-rename URLs.
  { path: 'dashboard', element: <LegacyDashboardRedirect to={paths.manager.dashboard} /> },
  { path: 'dashboard/listings', element: <LegacyDashboardRedirect to={paths.manager.properties} /> },
  { path: 'dashboard/listings/new', element: <LegacyDashboardRedirect to={paths.manager.newProperty} /> },
  { path: 'dashboard/listings/:propertyId/edit', element: <LegacyEditRedirect /> },
  { path: 'dashboard/leads', element: <LegacyDashboardRedirect to={paths.manager.leads} /> },
]);
