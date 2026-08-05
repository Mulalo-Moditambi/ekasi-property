import { useFormContext } from 'react-hook-form';
import { Field } from '../../../../shared/ui/form';
import { Input } from '../../../../shared/ui/input';
import { FormSection } from './FormSection';
import type { ListingFormValues } from '../../listingSchema';

export function RoomsStep() {
  const {
    register,
    formState: { errors },
  } = useFormContext<ListingFormValues>();

  return (
    <FormSection title="Rooms">
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="Bedrooms" error={errors.bedrooms?.message}>
          <Input
            type="number"
            min="0"
            className="tnum"
            aria-invalid={Boolean(errors.bedrooms)}
            {...register('bedrooms')}
          />
        </Field>

        <Field label="Bathrooms" error={errors.bathrooms?.message}>
          <Input
            type="number"
            min="0"
            className="tnum"
            aria-invalid={Boolean(errors.bathrooms)}
            {...register('bathrooms')}
          />
        </Field>
      </div>
    </FormSection>
  );
}
