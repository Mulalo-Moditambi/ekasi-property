import { Controller, useFormContext } from 'react-hook-form';
import { Field, Textarea } from '../../../../shared/ui/form';
import { Input } from '../../../../shared/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../../../../shared/ui/select';
import { FormSection } from './FormSection';
import type { ListingFormValues } from '../../listingSchema';
import { ListingType, PropertyType, listingTypeLabels, propertyTypeLabels } from '../../types';

/**
 * Steps read the form off context rather than through props, so adding a field
 * never means threading another prop through the page.
 */
export function BasicsStep({ isEdit }: { isEdit: boolean }) {
  const {
    control,
    register,
    watch,
    formState: { errors },
  } = useFormContext<ListingFormValues>();

  const listingType = watch('listingType');

  return (
    <FormSection title="The basics">
      {isEdit ? (
        <p className="rounded-lg bg-surface-2 px-3 py-2 text-sm text-ink-2">
          {listingTypeLabels[listingType as ListingType]} ·{' '}
          {propertyTypeLabels[watch('propertyType') as PropertyType]}
          <span className="text-ink-3"> — type can&apos;t be changed after publishing</span>
        </p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          <Controller
            control={control}
            name="listingType"
            render={({ field }) => (
              <Field label="Listing type">
                <Select value={String(field.value)} onValueChange={(v) => field.onChange(Number(v))}>
                  <SelectTrigger className="h-9 w-full rounded-lg">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={String(ListingType.Rent)}>To Rent</SelectItem>
                    <SelectItem value={String(ListingType.Sale)}>For Sale</SelectItem>
                  </SelectContent>
                </Select>
              </Field>
            )}
          />

          <Controller
            control={control}
            name="propertyType"
            render={({ field }) => (
              <Field label="Property type">
                <Select value={String(field.value)} onValueChange={(v) => field.onChange(Number(v))}>
                  <SelectTrigger className="h-9 w-full rounded-lg">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.entries(propertyTypeLabels).map(([value, label]) => (
                      <SelectItem key={value} value={value}>
                        {label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            )}
          />
        </div>
      )}

      <Field label="Title" error={errors.title?.message}>
        <Input
          type="text"
          maxLength={200}
          placeholder="e.g. Neat backroom with own entrance in Zola"
          aria-invalid={Boolean(errors.title)}
          {...register('title')}
        />
      </Field>

      <Field label="Description" error={errors.description?.message}>
        <Textarea
          maxLength={4000}
          rows={5}
          placeholder="Describe the place: size, electricity setup, water, entrance, rules…"
          aria-invalid={Boolean(errors.description)}
          {...register('description')}
        />
      </Field>

      <Field
        label={listingType === ListingType.Rent ? 'Monthly rent (R)' : 'Asking price (R)'}
        error={errors.price?.message}
      >
        <Input
          type="number"
          min="1"
          className="tnum"
          aria-invalid={Boolean(errors.price)}
          {...register('price')}
        />
      </Field>
    </FormSection>
  );
}
