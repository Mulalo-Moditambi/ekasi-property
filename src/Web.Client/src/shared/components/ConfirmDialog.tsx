import * as DialogPrimitive from '@radix-ui/react-dialog';
import { Button } from '../ui/button';

interface ConfirmDialogProps {
  title: string;
  message: string;
  confirmLabel?: string;
  danger?: boolean;
  busy?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDialog({
  title,
  message,
  confirmLabel = 'Confirm',
  danger = false,
  busy = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  return (
    <DialogPrimitive.Root open onOpenChange={(open) => !open && onCancel()}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="fixed inset-0 z-50 bg-slate-950/50 backdrop-blur-[2px]" />
        <DialogPrimitive.Content className="fixed left-1/2 top-1/2 z-50 w-[calc(100vw-2rem)] max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-line bg-surface p-5 shadow-pop outline-none">
          <DialogPrimitive.Title className="text-lg font-semibold tracking-tight text-ink">
            {title}
          </DialogPrimitive.Title>
          <DialogPrimitive.Description className="mt-2 text-sm leading-relaxed text-ink-2">
            {message}
          </DialogPrimitive.Description>

          <div className="mt-5 flex justify-end gap-2">
            <Button variant="outline" onClick={onCancel} disabled={busy}>
              Cancel
            </Button>
            <Button variant={danger ? 'danger' : 'cta'} onClick={onConfirm} disabled={busy}>
              {busy ? 'Working…' : confirmLabel}
            </Button>
          </div>
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  );
}
