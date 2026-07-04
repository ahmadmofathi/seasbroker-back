import {
  adminCreate,
  adminGetOne,
  adminList,
  adminRequest,
  adminUpdate,
} from './adminClient';
import type { CargoListingRecord, PromoteFromQuoteBody } from './types';

export async function listCargoListings(
  filter?: string,
  page = 1,
  perPage = 50,
): Promise<CargoListingRecord[]> {
  return adminList<CargoListingRecord>('cargoListings', { page, perPage, filter });
}

export async function getCargoListing(id: string): Promise<CargoListingRecord> {
  return adminGetOne<CargoListingRecord>('cargoListings', id);
}

export async function createCargoListing(
  body: Partial<CargoListingRecord>,
): Promise<CargoListingRecord> {
  return adminCreate<CargoListingRecord>('cargoListings', body);
}

export async function updateCargoListing(
  id: string,
  body: Partial<CargoListingRecord>,
): Promise<CargoListingRecord> {
  return adminUpdate<CargoListingRecord>('cargoListings', id, body);
}

export async function promoteFromQuote(body: PromoteFromQuoteBody): Promise<unknown> {
  return adminRequest('/api/cargo/promote-from-quote', { method: 'POST', body });
}

export async function closeCargo(id: string): Promise<unknown> {
  return adminRequest(`/api/cargo/${id}/close`, { method: 'POST' });
}

export async function cancelCargo(id: string): Promise<unknown> {
  return adminRequest(`/api/cargo/${id}/cancel`, { method: 'POST' });
}
