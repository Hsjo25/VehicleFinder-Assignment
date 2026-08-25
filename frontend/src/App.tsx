import { useEffect, useState } from 'react';
import './App.css';
import { SearchForm } from './components/SearchForm';
import { ModelResults } from './components/ModelResults';
import { ApiError, getMakes, getVehicleTypes, searchModels } from './services/api';
import { MAX_MODEL_YEAR } from './constants';
import type { Make, VehicleModel, VehicleType } from './types/vehicle';

function errorMessage(err: unknown, fallback: string): string {
  if (err instanceof ApiError) return err.message;
  return fallback;
}

function App() {
  const [makes, setMakes] = useState<Make[]>([]);
  const [makesLoading, setMakesLoading] = useState(true);
  const [makesError, setMakesError] = useState<string | null>(null);
  const [makesReloadKey, setMakesReloadKey] = useState(0);

  const [selectedMakeId, setSelectedMakeId] = useState<number | ''>('');
  const [vehicleTypes, setVehicleTypes] = useState<VehicleType[]>([]);
  const [vehicleTypesLoading, setVehicleTypesLoading] = useState(false);
  const [vehicleTypesError, setVehicleTypesError] = useState<string | null>(null);
  const [vehicleTypesReloadKey, setVehicleTypesReloadKey] = useState(0);

  const [selectedVehicleType, setSelectedVehicleType] = useState('');
  const [year, setYear] = useState<number | ''>(new Date().getFullYear());

  const [models, setModels] = useState<VehicleModel[] | null>(null);
  const [modelsLoading, setModelsLoading] = useState(false);
  const [modelsError, setModelsError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    setMakesLoading(true);
    setMakesError(null);

    getMakes(controller.signal)
      .then(setMakes)
      .catch((err) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setMakesError(errorMessage(err, 'Unable to load vehicle makes. Please try again.'));
      })
      .finally(() => setMakesLoading(false));

    return () => controller.abort();
  }, [makesReloadKey]);

  useEffect(() => {
    if (selectedMakeId === '') {
      setVehicleTypes([]);
      setVehicleTypesError(null);
      return;
    }

    const controller = new AbortController();
    setVehicleTypesLoading(true);
    setVehicleTypesError(null);

    getVehicleTypes(selectedMakeId, controller.signal)
      .then(setVehicleTypes)
      .catch((err) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setVehicleTypesError(errorMessage(err, 'Unable to load vehicle types. Please try again.'));
      })
      .finally(() => setVehicleTypesLoading(false));

    return () => controller.abort();
  }, [selectedMakeId, vehicleTypesReloadKey]);

  function handleMakeChange(makeId: number | '') {
    setSelectedMakeId(makeId);
    setSelectedVehicleType('');
    setModels(null);
    setModelsError(null);
  }

  function runSearch() {
    if (selectedMakeId === '' || year === '' || !selectedVehicleType) return;

    const controller = new AbortController();
    setModelsLoading(true);
    setModelsError(null);

    searchModels(selectedMakeId, year, selectedVehicleType, controller.signal)
      .then(setModels)
      .catch((err) => {
        if (err instanceof DOMException && err.name === 'AbortError') return;
        setModelsError(errorMessage(err, 'Vehicle data is temporarily unavailable.'));
      })
      .finally(() => setModelsLoading(false));

    return () => controller.abort();
  }

  const isYearValid = year !== '' && year <= MAX_MODEL_YEAR;
  const canSubmit =
    selectedMakeId !== '' && isYearValid && !!selectedVehicleType && !modelsLoading;

  const selectedMakeName = makes.find((m) => m.id === selectedMakeId)?.name ?? '';

  return (
    <div className="page">
      <header className="page-header">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true">
            <svg viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path
                d="M8 27 L10.5 18.5 Q11.5 15 15.5 15 H32.5 Q36.5 15 37.5 18.5 L40 27"
                stroke="currentColor"
                strokeWidth="2.6"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
              <rect x="5" y="25" width="38" height="10" rx="5" fill="currentColor" />
              <circle cx="15" cy="36" r="4.2" fill="var(--color-surface)" stroke="currentColor" strokeWidth="2.6" />
              <circle cx="33" cy="36" r="4.2" fill="var(--color-surface)" stroke="currentColor" strokeWidth="2.6" />
            </svg>
          </span>
          <h1>Vehicle Finder</h1>
        </div>
        <p>Find vehicle models by manufacturer, year and vehicle type.</p>
      </header>

      <main className="content">
        <section className="card">
          <SearchForm
            makes={makes}
            makesLoading={makesLoading}
            makesError={makesError}
            onRetryMakes={() => setMakesReloadKey((key) => key + 1)}
            selectedMakeId={selectedMakeId}
            onMakeChange={handleMakeChange}
            vehicleTypes={vehicleTypes}
            vehicleTypesLoading={vehicleTypesLoading}
            vehicleTypesError={vehicleTypesError}
            onRetryVehicleTypes={() => setVehicleTypesReloadKey((key) => key + 1)}
            selectedVehicleType={selectedVehicleType}
            onVehicleTypeChange={setSelectedVehicleType}
            year={year}
            onYearChange={setYear}
            onSubmit={runSearch}
            canSubmit={canSubmit}
            searching={modelsLoading}
          />
        </section>

        <ModelResults
          models={models}
          loading={modelsLoading}
          error={modelsError}
          onRetry={runSearch}
          makeName={selectedMakeName}
          year={year}
          vehicleType={selectedVehicleType}
        />
      </main>
    </div>
  );
}

export default App;
