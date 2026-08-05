import { Controller, useFormContext } from 'react-hook-form';
import { Checkbox } from '../../../../shared/ui/checkbox';
import { FormSection } from './FormSection';
import type { ListingFormValues } from '../../listingSchema';

const AMENITIES = [
  { key: 'hasElectricity', label: 'Electricity (own or prepaid meter)' },
  { key: 'waterIncluded', label: 'Water included' },
  { key: 'hasOwnEntrance', label: 'Own entrance' },
  { key: 'hasParking', label: 'Parking' },
] as const;

export function AmenitiesStep() {
  const { control } = useFormContext<ListingFormValues>();

  return (
    <FormSection title="Amenities">
      <div className="grid gap-3 sm:grid-cols-2">
        {AMENITIES.map(({ key, label }) => (
          <Controller
            key={key}
            control={control}
            name={key}
            render={({ field }) => (
              <label className="flex cursor-pointer items-center gap-2.5 text-sm text-ink">
                <Checkbox
                  checked={field.value}
                  onCheckedChange={(checked) => field.onChange(checked === true)}
                />
                {label}
              </label>
            )}
          />
        ))}
      </div>
    </FormSection>
  );
}
