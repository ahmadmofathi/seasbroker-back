import {
  adminCreate,
  adminDelete,
  adminGetOne,
  adminList,
  adminRequest,
  adminUpdate,
} from './adminClient';
import type { PromoteToVesselResult, VesselAvailabilityRecord, VesselRecord } from './types';

/** Adds the vessel from a Ship Brokerage request to the fleet, with an availability window. */
export async function promoteFromQuote(requestedQuoteId: string): Promise<PromoteToVesselResult> {
  return adminRequest<PromoteToVesselResult>('/api/vessels/promote-from-quote', {
    method: 'POST',
    body: { requestedQuoteId },
  });
}

export async function listVessels(
  filter?: string,
  page = 1,
  perPage = 50,
): Promise<VesselRecord[]> {
  return adminList<VesselRecord>('vessels', { page, perPage, filter });
}

export async function getVessel(id: string): Promise<VesselRecord> {
  return adminGetOne<VesselRecord>('vessels', id);
}

export async function createVessel(body: Partial<VesselRecord>): Promise<VesselRecord> {
  return adminCreate<VesselRecord>('vessels', body);
}

export async function updateVessel(id: string, body: Partial<VesselRecord>): Promise<VesselRecord> {
  return adminUpdate<VesselRecord>('vessels', id, body);
}

export async function deleteVessel(id: string): Promise<void> {
  return adminDelete('vessels', id);
}

export async function listVesselAvailabilities(
  vesselId: string,
): Promise<VesselAvailabilityRecord[]> {
  return adminList<VesselAvailabilityRecord>('vesselAvailabilities', {
    filter: `vesselId = "${vesselId}"`,
  });
}

export async function createVesselAvailability(
  body: Partial<VesselAvailabilityRecord>,
): Promise<VesselAvailabilityRecord> {
  return adminCreate<VesselAvailabilityRecord>('vesselAvailabilities', body);
}

export async function updateVesselAvailability(
  id: string,
  body: Partial<VesselAvailabilityRecord>,
): Promise<VesselAvailabilityRecord> {
  return adminUpdate<VesselAvailabilityRecord>('vesselAvailabilities', id, body);
}

export async function deleteVesselAvailability(id: string): Promise<void> {
  return adminDelete('vesselAvailabilities', id);
}
