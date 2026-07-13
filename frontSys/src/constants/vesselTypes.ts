/** Must match Seasbroker.Modules.Vessel.Application.Constants.VesselConstants.AllowedVesselTypes */
export const ALLOWED_VESSEL_TYPES = [
  'Bulk',
  'Container',
  'RoRo',
  'Tanker',
  'General Cargo',
  'LNG',
  'LPG',
] as const;

export type AllowedVesselType = (typeof ALLOWED_VESSEL_TYPES)[number];

export const DEFAULT_VESSEL_TYPE: AllowedVesselType = 'Bulk';

/** Options for selects; keeps unknown legacy values when editing existing records. */
export function vesselTypeSelectOptions(current?: string): string[] {
  const options = [...ALLOWED_VESSEL_TYPES];
  const trimmed = current?.trim();
  if (trimmed && !options.includes(trimmed as AllowedVesselType)) {
    return [trimmed, ...options];
  }
  return options;
}

export function toVesselTypeOptions(): Array<{ value: string; text: string }> {
  return ALLOWED_VESSEL_TYPES.map((type) => ({ value: type, text: type }));
}
