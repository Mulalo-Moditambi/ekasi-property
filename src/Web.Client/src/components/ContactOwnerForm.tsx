import { useState } from 'react';
import type { FormEvent } from 'react';
import { submitInquiry } from '../api/inquiries';

export function ContactOwnerForm({ propertyId }: { propertyId: string }) {
  const [form, setForm] = useState({ name: '', email: '', phone: '', message: '' });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [sent, setSent] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setBusy(true);
    setError(null);

    try {
      await submitInquiry(propertyId, {
        name: form.name,
        email: form.email,
        phone: form.phone || undefined,
        message: form.message,
      });
      setSent(true);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not send your message');
      setBusy(false);
    }
  }

  if (sent) {
    return (
      <section className="contact-owner">
        <h2>Contact the owner</h2>
        <p className="success">Your message was sent. The owner will get back to you on the details you provided.</p>
      </section>
    );
  }

  return (
    <section className="contact-owner">
      <h2>Contact the owner</h2>
      <form className="stacked-form" onSubmit={handleSubmit}>
        <label>
          Your name
          <input
            type="text"
            required
            maxLength={100}
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
          />
        </label>
        <label>
          Email
          <input
            type="email"
            required
            maxLength={255}
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
        </label>
        <label>
          Phone (optional)
          <input
            type="tel"
            maxLength={20}
            value={form.phone}
            onChange={(e) => setForm({ ...form, phone: e.target.value })}
          />
        </label>
        <label>
          Message
          <textarea
            required
            maxLength={2000}
            rows={4}
            placeholder="e.g. Is this still available? When can I come view it?"
            value={form.message}
            onChange={(e) => setForm({ ...form, message: e.target.value })}
          />
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" disabled={busy}>
          {busy ? 'Sending…' : 'Send message'}
        </button>
      </form>
    </section>
  );
}
