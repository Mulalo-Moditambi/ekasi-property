import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { CheckCircle2, Send } from 'lucide-react';
import { errorMessage } from '../../../shared/api/queryClient';
import { Button } from '../../../shared/ui/button';
import { Field, Notice, Textarea } from '../../../shared/ui/form';
import { Input } from '../../../shared/ui/input';
import { emptyInquiry, inquirySchema, toInquiryRequest } from '../inquirySchema';
import type { InquiryFormValues } from '../inquirySchema';
import { useSubmitInquiry } from '../queries';

export function ContactOwnerForm({ propertyId }: { propertyId: string }) {
  const submit = useSubmitInquiry(propertyId);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<InquiryFormValues>({
    resolver: zodResolver(inquirySchema),
    defaultValues: emptyInquiry,
    mode: 'onTouched',
  });

  async function onValid(values: InquiryFormValues) {
    try {
      await submit.mutateAsync(toInquiryRequest(values));
    } catch (error) {
      setError('root', { message: errorMessage(error, 'Could not send your message') });
    }
  }

  if (submit.isSuccess) {
    return (
      <div className="flex flex-col items-center gap-2 py-4 text-center">
        <CheckCircle2 className="size-8 text-cta" />
        <h2 className="text-base font-semibold text-ink">Message sent</h2>
        <p className="text-sm leading-relaxed text-ink-2">
          The owner will get back to you on the details you provided.
        </p>
      </div>
    );
  }

  return (
    <div>
      <h2 className="text-base font-semibold tracking-tight text-ink">Contact the owner</h2>
      <p className="mt-1 text-sm text-ink-2">
        No agents, no commission — you deal with them directly.
      </p>

      <form className="mt-4 space-y-3" noValidate onSubmit={handleSubmit(onValid)}>
        <Field label="Your name" error={errors.name?.message}>
          <Input
            type="text"
            maxLength={100}
            autoComplete="name"
            aria-invalid={Boolean(errors.name)}
            {...register('name')}
          />
        </Field>

        <Field label="Email" error={errors.email?.message}>
          <Input
            type="email"
            maxLength={255}
            autoComplete="email"
            aria-invalid={Boolean(errors.email)}
            {...register('email')}
          />
        </Field>

        <Field label="Phone" hint="Optional" error={errors.phone?.message}>
          <Input
            type="tel"
            maxLength={20}
            autoComplete="tel"
            aria-invalid={Boolean(errors.phone)}
            {...register('phone')}
          />
        </Field>

        <Field label="Message" error={errors.message?.message}>
          <Textarea
            maxLength={2000}
            rows={4}
            placeholder="e.g. Is this still available? When can I come view it?"
            aria-invalid={Boolean(errors.message)}
            {...register('message')}
          />
        </Field>

        {errors.root && <Notice>{errors.root.message}</Notice>}

        <Button type="submit" variant="cta" size="lg" className="w-full" disabled={isSubmitting}>
          <Send />
          {isSubmitting ? 'Sending…' : 'Send message'}
        </Button>
      </form>
    </div>
  );
}
