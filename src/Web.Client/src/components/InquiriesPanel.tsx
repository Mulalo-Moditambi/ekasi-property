import { useEffect, useState } from 'react';
import { getInquiries } from '../api/inquiries';
import type { Inquiry } from '../types/inquiry';

export function InquiriesPanel({ propertyId }: { propertyId: string }) {
  const [inquiries, setInquiries] = useState<Inquiry[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getInquiries(propertyId)
      .then(setInquiries)
      .catch((e: unknown) => setError(e instanceof Error ? e.message : 'Could not load inquiries'));
  }, [propertyId]);

  return (
    <section className="inquiries-panel">
      <h2>Inquiries</h2>
      {error && <p className="error">{error}</p>}
      {!error && inquiries === null && <p className="muted">Loading inquiries…</p>}
      {inquiries !== null && inquiries.length === 0 && (
        <p className="muted">No inquiries yet. When someone contacts you about this listing, it shows up here.</p>
      )}
      {inquiries !== null && inquiries.length > 0 && (
        <ul className="inquiry-list">
          {inquiries.map((inquiry) => (
            <li key={inquiry.id}>
              <div className="inquiry-head">
                <strong>{inquiry.name}</strong>
                <span className="muted">{new Date(inquiry.createdAt).toLocaleString()}</span>
              </div>
              <p className="inquiry-contact muted">
                <a href={`mailto:${inquiry.email}`}>{inquiry.email}</a>
                {inquiry.phone && <> · <a href={`tel:${inquiry.phone}`}>{inquiry.phone}</a></>}
              </p>
              <p>{inquiry.message}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
