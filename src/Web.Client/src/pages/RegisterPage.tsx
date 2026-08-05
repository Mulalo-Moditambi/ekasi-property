import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { register } from '../features/auth/api';
import { useAuth } from '../features/auth/AuthContext';
import { AuthLayout } from '../shared/layout/AuthLayout';
import { Button } from '../shared/ui/button';
import { Field, Notice } from '../shared/ui/form';
import { Input } from '../shared/ui/input';

export function RegisterPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? '/';

  const [form, setForm] = useState({ email: '', firstName: '', lastName: '', password: '' });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);

    try {
      await register(form.email, form.firstName, form.lastName, form.password);
      await login(form.email, form.password);
      navigate(from, { replace: true });
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Registration failed');
      setBusy(false);
    }
  }

  return (
    <AuthLayout
      title="Create your account"
      subtitle="List a property in minutes — it's free."
      footer={
        <>
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-cta underline-offset-4 hover:underline">
            Log in
          </Link>
        </>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label="First name">
            <Input
              type="text"
              autoComplete="given-name"
              required
              value={form.firstName}
              onChange={(e) => setForm({ ...form, firstName: e.target.value })}
            />
          </Field>
          <Field label="Last name">
            <Input
              type="text"
              autoComplete="family-name"
              required
              value={form.lastName}
              onChange={(e) => setForm({ ...form, lastName: e.target.value })}
            />
          </Field>
        </div>

        <Field label="Email">
          <Input
            type="email"
            autoComplete="email"
            required
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
        </Field>

        <Field label="Password" hint="At least 8 characters">
          <Input
            type="password"
            autoComplete="new-password"
            required
            minLength={8}
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
          />
        </Field>

        {error && <Notice>{error}</Notice>}

        <Button type="submit" variant="cta" size="lg" className="w-full" disabled={busy}>
          {busy ? 'Creating account…' : 'Create account'}
        </Button>
      </form>
    </AuthLayout>
  );
}
