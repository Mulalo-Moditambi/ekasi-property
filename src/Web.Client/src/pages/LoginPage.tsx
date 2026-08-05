import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { REDIRECT_PARAM, paths, safeRedirectTarget } from '../app/routes/paths';
import { useAuth } from '../features/auth/AuthContext';
import { AuthLayout } from '../shared/layout/AuthLayout';
import { Button } from '../shared/ui/button';
import { Field, Notice } from '../shared/ui/form';
import { Input } from '../shared/ui/input';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  // Where the guard wanted to send them. Validated, so a crafted
  // `?redirectUrl=https://…` can't bounce a freshly-authenticated user off-site.
  const destination = safeRedirectTarget(searchParams.get(REDIRECT_PARAM));

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);

    try {
      await login(email, password);
      navigate(destination, { replace: true });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed');
      setBusy(false);
    }
  }

  return (
    <AuthLayout
      title="Welcome back"
      subtitle="Log in to manage your listings and leads."
      footer={
        <>
          New here?{' '}
          <Link
            to={paths.auth.register}
            className="font-medium text-cta underline-offset-4 hover:underline"
          >
            Create an account
          </Link>{' '}
          to list your property.
        </>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        <Field label="Email">
          <Input
            type="email"
            autoComplete="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </Field>
        <Field label="Password">
          <Input
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </Field>

        {error && <Notice>{error}</Notice>}

        <Button type="submit" variant="cta" size="lg" className="w-full" disabled={busy}>
          {busy ? 'Logging in…' : 'Log in'}
        </Button>
      </form>
    </AuthLayout>
  );
}
