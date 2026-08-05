/**
 * Every URL in the app is built from here. Nothing else in the codebase should
 * hand-write a path string: when a segment moves, this file is the only edit,
 * and TypeScript finds the call sites for you.
 */

export const paths = {
  home: '/',

  properties: {
    root: '/properties',
    detail: (propertyId: string) => `/properties/${propertyId}`,
    overview: (propertyId: string) => `/properties/${propertyId}/overview`,
    amenities: (propertyId: string) => `/properties/${propertyId}/amenities`,
    location: (propertyId: string) => `/properties/${propertyId}/location`,
  },

  auth: {
    login: '/login',
    register: '/register',
  },

  /**
   * Owner-side portal. Named "manager" rather than "dashboard" so the segment
   * says who the space belongs to — `/manager/properties` reads as a portfolio,
   * `/dashboard/listings` only made sense if you already knew the app.
   */
  manager: {
    root: '/manager',
    dashboard: '/manager/dashboard',
    properties: '/manager/properties',
    newProperty: '/manager/properties/new',
    editProperty: (propertyId: string) => `/manager/properties/${propertyId}/edit`,
    leads: '/manager/leads',
  },

  forbidden: '/403',
} as const;

/** The query key that carries the post-login destination. */
export const REDIRECT_PARAM = 'redirectUrl';

/** `/login?redirectUrl=%2Fmanager%2Fproperties` — one place builds it, one place reads it. */
export function loginWithRedirect(target: string): string {
  return `${paths.auth.login}?${REDIRECT_PARAM}=${encodeURIComponent(target)}`;
}

/**
 * Only same-origin, absolute-path redirects are honoured. A `redirectUrl` of
 * `https://evil.example` or `//evil.example` would otherwise turn the login
 * screen into an open redirect.
 */
export function safeRedirectTarget(raw: string | null): string {
  if (!raw) {
    return paths.manager.dashboard;
  }

  return raw.startsWith('/') && !raw.startsWith('//') ? raw : paths.manager.dashboard;
}
