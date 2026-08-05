import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { Check, TriangleAlert, X } from 'lucide-react';
import { cn } from '../ui/utils';

interface ToastItem {
  id: number;
  message: string;
  tone: 'success' | 'error';
}

interface ToastContextValue {
  notify: (message: string, tone?: 'success' | 'error') => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

let nextId = 1;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const dismiss = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id));
  }, []);

  const notify = useCallback(
    (message: string, tone: 'success' | 'error' = 'success') => {
      const id = nextId++;
      setToasts((current) => [...current, { id, message, tone }]);
      setTimeout(() => dismiss(id), 4000);
    },
    [dismiss],
  );

  const value = useMemo(() => ({ notify }), [notify]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div
        className="pointer-events-none fixed inset-x-0 bottom-0 z-[60] flex flex-col items-center gap-2 p-4 sm:items-end"
        role="status"
        aria-live="polite"
      >
        {toasts.map((toast) => (
          <div
            key={toast.id}
            className={cn(
              'pointer-events-auto flex w-full max-w-sm items-start gap-2.5 rounded-xl border px-3.5 py-3 shadow-pop',
              'bg-surface text-sm',
              toast.tone === 'success' ? 'border-cta-line' : 'border-danger/30',
            )}
          >
            {toast.tone === 'success' ? (
              <Check className="mt-0.5 size-4 shrink-0 text-cta" strokeWidth={3} />
            ) : (
              <TriangleAlert className="mt-0.5 size-4 shrink-0 text-danger" />
            )}
            <span className="flex-1 text-ink">{toast.message}</span>
            <button
              type="button"
              onClick={() => dismiss(toast.id)}
              aria-label="Dismiss"
              className="rounded-md p-0.5 text-ink-3 transition-colors hover:text-ink focus-visible:outline-2 focus-visible:outline-cta"
            >
              <X className="size-3.5" />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const context = useContext(ToastContext);

  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }

  return context;
}
