import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { register } from '../features/auth/api';
import { useAuth } from '../features/auth/AuthContext';

export function RegisterPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

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
      navigate('/');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Registration failed');
      setBusy(false);
    }
  }

  return (
    <div className="page narrow">
      <h1>Create an account</h1>
      <form className="stacked-form" onSubmit={handleSubmit}>
        <label>
          First name
          <input
            type="text"
            required
            value={form.firstName}
            onChange={(e) => setForm({ ...form, firstName: e.target.value })}
          />
        </label>
        <label>
          Last name
          <input
            type="text"
            required
            value={form.lastName}
            onChange={(e) => setForm({ ...form, lastName: e.target.value })}
          />
        </label>
        <label>
          Email
          <input
            type="email"
            required
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
        </label>
        <label>
          Password
          <input
            type="password"
            required
            minLength={8}
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
          />
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" disabled={busy}>
          {busy ? 'Creating account…' : 'Register'}
        </button>
      </form>
    </div>
  );
}
