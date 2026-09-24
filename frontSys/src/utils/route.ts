import type { RouteStopValue } from '../api/types';
import { isKnownPort } from './portOptions';

export function stopLabel(index: number): string {
  return index === 0 ? 'Next port' : `Port ${String(index + 1)}`;
}

/** Client-side mirror of the backend's route checks. Returns the first problem, or null. */
export function validateRoute(
  stops: RouteStopValue[],
  options: { minStops?: number | null; noPastDates?: boolean | null } = {},
): string | null {
  for (let i = 0; i < stops.length; i++) {
    const { port, eta } = stops[i];
    const label = stopLabel(i);
    if (!isKnownPort(port)) return `${label}: pick a port from the list.`;
    if (!eta) return `${label}: set an ETA.`;
    if (options.noPastDates) {
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (new Date(eta) < today) return `${label}: ETA cannot be in the past.`;
    }
    if (i > 0) {
      if (port === stops[i - 1].port) return `${label} repeats the port before it.`;
      // same input type on every row, so ISO strings compare correctly
      if (eta <= stops[i - 1].eta) return `${label}: ETA must be after the previous port's ETA.`;
    }
  }
  if (options.minStops != null && stops.length < options.minStops) {
    return `Add at least ${String(options.minStops)} ports to the route.`;
  }
  return null;
}
