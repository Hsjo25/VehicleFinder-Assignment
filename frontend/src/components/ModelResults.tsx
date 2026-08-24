import type { VehicleModel } from '../types/vehicle';
import { InlineError } from './InlineError';

interface ModelResultsProps {
  models: VehicleModel[] | null;
  loading: boolean;
  error: string | null;
  onRetry: () => void;
}

export function ModelResults({ models, loading, error, onRetry }: ModelResultsProps) {
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

  if (models.length === 0) {
    return (
      <div className="results-panel status">
        <p>No models were found for the selected criteria.</p>
      </div>
    );
  }

  return (
    <div className="results-panel">
      <div className="results-header">
        <h2>Models Found</h2>
        <p>{models.length} vehicle{models.length === 1 ? '' : 's'}</p>
      </div>
      <ul className="results-grid">
        {models.map((model) => (
          <li key={model.id} className="model-card">
            <span className="model-make">{model.makeName}</span>
            <span className="model-name">{model.name}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
