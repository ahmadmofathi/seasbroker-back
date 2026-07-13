import type { CargoStatus, VesselStatus } from '../api/types';

/** Seasbroker.Modules.Cargo.Application.Constants.CargoConstants */
export const CARGO_PRIORITIES = [1, 2, 3, 4, 5] as const;
export const DEFAULT_CARGO_PRIORITY = 3;

export const CARGO_STATUSES: CargoStatus[] = [
  'Draft',
  'Open',
  'Matched',
  'Closed',
  'Cancelled',
];

export const CARGO_STATUSES_ON_CREATE: CargoStatus[] = ['Draft', 'Open'];

export const VESSEL_STATUSES: VesselStatus[] = ['Active', 'Inactive', 'Maintenance'];
