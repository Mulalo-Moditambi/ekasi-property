import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import type { ReactNode } from 'react';
import { AuthProvider, useAuth } from './features/auth/AuthContext';
import { Layout } from './shared/layout/Layout';
import { CreateListingPage } from './pages/CreateListingPage';
import { LoginPage } from './pages/LoginPage';
import { PropertyDetailPage } from './pages/PropertyDetailPage';
import { RegisterPage } from './pages/RegisterPage';
import { SearchPage } from './pages/SearchPage';

function RequireAuth({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? children : <Navigate to="/login" replace />;
}

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route index element={<SearchPage />} />
            <Route path="properties/:id" element={<PropertyDetailPage />} />
            <Route
              path="list-property"
              element={
                <RequireAuth>
                  <CreateListingPage />
                </RequireAuth>
              }
            />
            <Route path="login" element={<LoginPage />} />
            <Route path="register" element={<RegisterPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
