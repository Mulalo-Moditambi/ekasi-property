import { useFormContext } from 'react-hook-form';
import { Field } from '../../../../shared/ui/form';
import { Input } from '../../../../shared/ui/input';
import { FormSection } from './FormSection';
import type { ListingFormValues } from '../../listingSchema';

export function AddressStep() {
  const {
    register,
    formState: { errors },
  } = useFormContext<ListingFormValues>();

  return (
    <FormSection title="Address">
      <Field label="Street" error={errors.street?.message}>
        <Input type="text" aria-invalid={Boolean(errors.street)} {...register('street')} />
      </Field>

      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="Township / Section" error={errors.township?.message}>
          <Input
            type="text"
            placeholder="e.g. Orlando West"
            aria-invalid={Boolean(errors.township)}
            {...register('township')}
          />
        </Field>

        <Field label="City" error={errors.city?.message}>
          <Input
            type="text"
            placeholder="e.g. Soweto"
            aria-invalid={Boolean(errors.city)}
            {...register('city')}
          />
        </Field>

        <Field label="Province" error={errors.province?.message}>
          <Input
            type="text"
            placeholder="e.g. Gauteng"
            aria-invalid={Boolean(errors.province)}
            {...register('province')}
          />
        </Field>

        <Field label="Postal code" error={errors.postalCode?.message}>
          <Input
            type="text"
            inputMode="numeric"
            maxLength={4}
            className="tnum"
            aria-invalid={Boolean(errors.postalCode)}
            {...register('postalCode')}
          />
        </Field>
      </div>
    </FormSection>
  );
}
