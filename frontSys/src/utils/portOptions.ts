import ports from './ports.json';

export interface PortOption {
  value: string;
  label: string;
  country: string;
}

let cachedPorts: PortOption[] | null = null;
let cachedCountries: string[] | null = null;

function buildPortOptions(): PortOption[] {
  const seen = new Set<string>();
  const options: PortOption[] = [];

  for (const port of Object.values(ports)) {
    const label = `${port.name} - ${port.country}`;
    if (seen.has(label)) continue;
    seen.add(label);
    options.push({ value: label, label, country: port.country });
  }

  return options.sort((a, b) => a.label.localeCompare(b.label));
}

export function getPortOptions(): PortOption[] {
  cachedPorts ??= buildPortOptions();
  return cachedPorts;
}

export function getCountryOptions(): string[] {
  if (cachedCountries) return cachedCountries;
  const countries = new Set<string>();
  for (const port of Object.values(ports)) {
    if (port.country) countries.add(port.country);
  }
  cachedCountries = [...countries].sort((a, b) => a.localeCompare(b));
  return cachedCountries;
}

export function isKnownPort(value?: string): boolean {
  const trimmed = value?.trim();
  if (!trimmed) return false;
  return getPortOptions().some((o) => o.value === trimmed);
}

/** Keeps unknown legacy port strings when editing existing records. */
export function portSelectOptions(current?: string): PortOption[] {
  const options = getPortOptions();
  const trimmed = current?.trim();
  if (trimmed && isKnownPort(trimmed)) {
    return options;
  }
  if (trimmed && !options.some((o) => o.value === trimmed)) {
    return [{ value: trimmed, label: `${trimmed} (invalid — pick a port)`, country: '' }, ...options];
  }
  return options;
}

export function countrySelectOptions(current?: string): string[] {
  const options = getCountryOptions();
  const trimmed = current?.trim();
  if (trimmed && !options.includes(trimmed)) {
    return [trimmed, ...options];
  }
  return options;
}
