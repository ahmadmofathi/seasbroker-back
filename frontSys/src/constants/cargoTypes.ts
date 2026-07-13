/** Cargo types used across quotes, listings, and matching rules */
export const ALLOWED_CARGO_TYPES = [
  'Bulk',
  'Container',
  'RoRo',
  'Tanker',
  'General Cargo',
  'LNG',
  'LPG',
  'Gas',
  'Liquid',
  'Heavy',
  'Fragile',
  'Perishable',
  'Dry',
  'Refrigerated',
  'Other',
] as const;

export type AllowedCargoType = (typeof ALLOWED_CARGO_TYPES)[number];

export const DEFAULT_CARGO_TYPE: AllowedCargoType = 'Bulk';

export function cargoTypeSelectOptions(current?: string): string[] {
  const options = [...ALLOWED_CARGO_TYPES];
  const trimmed = current?.trim();
  if (trimmed && !options.includes(trimmed as AllowedCargoType)) {
    return [trimmed, ...options];
  }
  return options;
}

export function toCargoTypeOptions(): Array<{ value: string; text: string }> {
  return ALLOWED_CARGO_TYPES.map((type) => ({ value: type, text: type }));
}
