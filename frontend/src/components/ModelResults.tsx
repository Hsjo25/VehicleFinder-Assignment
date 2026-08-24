import type { VehicleModel } from '../types/vehicle';
import { InlineError } from './InlineError';

interface ModelResultsProps {
  models: VehicleModel[] | null;
  loading: boolean;
  error: string | null;
  onRetry: () => void;
  makeName: string;
  year: number | '';
  vehicleType: string;
}

export function ModelResults({ models, loading, error, onRetry, makeName, year, vehicleType }: ModelResultsProps) {
  if (loading) {
    return (
      <div className="results-panel status" aria-live="polite">
        <div className="spinner" aria-hidden="true" />
        <p>Searching vehicles…</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="results-panel status">
        <InlineError message={error} onRetry={onRetry} />
      </div>
    );
  }

  if (models === null) {
    return null;
  }

  const criteria = [makeName, year, vehicleType].filter(Boolean).join(' · ');

  if (models.length === 0) {
    return (
      <div className="results-panel status">
        <p>No models were found for {criteria || 'the selected criteria'}.</p>
      </div>
    );
  }

  return (
    <div className="results-panel">
      <div className="results-header">
        <div>
          <h2>Models Found</h2>
          {criteria && <p className="results-criteria">{criteria}</p>}
        </div>
        <span className="results-count">{models.length} vehicle{models.length === 1 ? '' : 's'}</span>
      </div>
      <ul className="results-grid">
        {models.map((model) => (
          <li key={model.id} className="model-card">
            {model.name}
          </li>
        ))}
      </ul>
    </div>
  );
}
