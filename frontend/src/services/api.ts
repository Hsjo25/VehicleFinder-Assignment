import type { Make, VehicleModel, VehicleType } from '../types/vehicle';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

export class ApiError extends Error {}

async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { signal });
  } catch (err) {
    if (err instanceof DOMException && err.name === 'AbortError') {
      throw err;
    }
    throw new ApiError('Unable to reach the server. Please check your connection and try again.');
  }

  if (!response.ok) {
    let detail: string | undefined;
    try {
      const problem = await response.json();
      detail = problem?.detail ?? problem?.title;
    } catch {
      // Response body wasn't JSON — fall back to a generic message below.
    }
    throw new ApiError(detail ?? 'Something went wrong. Please try again.');
  }

  return (await response.json()) as T;
}

export function getMakes(signal?: AbortSignal): Promise<Make[]> {
  return getJson<Make[]>('/vehicles/makes', signal);
}

export function getVehicleTypes(makeId: number, signal?: AbortSignal): Promise<VehicleType[]> {
  return getJson<VehicleType[]>(`/vehicles/makes/${makeId}/types`, signal);
}

export function searchModels(
  makeId: number,
  year: number,
  vehicleType: string,
  signal?: AbortSignal,
): Promise<VehicleModel[]> {
  const params = new URLSearchParams({
    makeId: String(makeId),
    year: String(year),
    vehicleType,
  });
  return getJson<VehicleModel[]>(`/vehicles/models?${params.toString()}`, signal);
}
