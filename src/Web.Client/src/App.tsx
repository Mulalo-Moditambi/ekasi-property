import { useState } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { AuthProvider } from './features/auth/AuthContext';
import { ToastProvider } from './shared/components/Toast';
import { createQueryClient } from './shared/api/queryClient';
import { router } from './app/routes/router';

/**
 * Providers only — the route table lives in `app/routes/router.tsx`.
 *
 * The query client is created in state rather than at module scope so it is
 * never shared across React roots (tests, StrictMode's double-invoke) while
 * still surviving every re-render of this component.
 */
export function App() {
  const [queryClient] = useState(createQueryClient);

  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>
  );
}
