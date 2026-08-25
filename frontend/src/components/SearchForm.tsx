import { useMemo } from 'react';
import type { Make, VehicleType } from '../types/vehicle';
import { MAX_MODEL_YEAR } from '../constants';
import { InlineError } from './InlineError';
import { SearchableSelect } from './SearchableSelect';

interface SearchFormProps {
  makes: Make[];
  makesLoading: boolean;
  makesError: string | null;
  onRetryMakes: () => void;

  selectedMakeId: number | '';
  onMakeChange: (makeId: number | '') => void;

  vehicleTypes: VehicleType[];
  vehicleTypesLoading: boolean;
  vehicleTypesError: string | null;
  onRetryVehicleTypes: () => void;

  selectedVehicleType: string;
  onVehicleTypeChange: (vehicleType: string) => void;

  year: number | '';
  onYearChange: (year: number | '') => void;

  onSubmit: () => void;
  canSubmit: boolean;
  searching: boolean;
}

export function SearchForm({
  makes,
  makesLoading,
  makesError,
  onRetryMakes,
  selectedMakeId,
  onMakeChange,
  vehicleTypes,
  vehicleTypesLoading,
  vehicleTypesError,
  onRetryVehicleTypes,
  selectedVehicleType,
  onVehicleTypeChange,
  year,
  onYearChange,
  onSubmit,
  canSubmit,
  searching,
}: SearchFormProps) {
  const makeOptions = useMemo(() => makes.map((m) => ({ id: m.id, label: m.name })), [makes]);

  const yearError =
    year !== '' && (year <= 0 || year > MAX_MODEL_YEAR)
      ? `Enter a positive year no later than ${MAX_MODEL_YEAR}.`
      : null;

  return (
    <form
      className="search-form"
      onSubmit={(e) => {
        e.preventDefault();
        if (canSubmit) onSubmit();
      }}
    >
      <div className="search-form-fields">
        <div className="field">
          <label htmlFor="make-select">Make</label>
          <SearchableSelect
            id="make-select"
            options={makeOptions}
            value={selectedMakeId}
            onChange={onMakeChange}
            disabled={makesLoading || !!makesError}
            placeholder={makesLoading ? 'Loading makes…' : 'Search for a make…'}
          />
          {makesError && <InlineError message={makesError} onRetry={onRetryMakes} />}
        </div>

        <div className="field">
          <label htmlFor="year-input">Year</label>
          <input
            id="year-input"
            type="number"
            inputMode="numeric"
            min={1}
            max={MAX_MODEL_YEAR}
            value={year}
            aria-invalid={!!yearError}
            aria-describedby={yearError ? 'year-error' : 'year-hint'}
            onChange={(e) => onYearChange(e.target.value === '' ? '' : Number(e.target.value))}
          />
          {yearError ? (
            <p id="year-error" className="field-error" role="alert">{yearError}</p>
          ) : (
            <p id="year-hint" className="field-hint">Up to {MAX_MODEL_YEAR}</p>
          )}
        </div>

        <div className="field">
          <label htmlFor="vehicle-type-select">Vehicle Type</label>
          <select
            id="vehicle-type-select"
            value={selectedVehicleType}
            disabled={!selectedMakeId || vehicleTypesLoading || !!vehicleTypesError}
            onChange={(e) => onVehicleTypeChange(e.target.value)}
          >
            <option value="">
              {!selectedMakeId
                ? 'Select a make first'
                : vehicleTypesLoading
                  ? 'Loading vehicle types…'
                  : 'Select a vehicle type'}
            </option>
            {vehicleTypes.map((type) => (
              <option key={type.id} value={type.name}>
                {type.name}
              </option>
            ))}
          </select>
          {vehicleTypesError && <InlineError message={vehicleTypesError} onRetry={onRetryVehicleTypes} />}
        </div>
      </div>

      <div className="search-form-actions">
        <button type="submit" className="search-button" disabled={!canSubmit || searching}>
          {searching ? 'Searching…' : 'Search Vehicles'}
        </button>
      </div>
    </form>
  );
}
