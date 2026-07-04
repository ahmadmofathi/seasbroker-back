import { adminList } from './adminClient';
import { api } from './client';
import type { QuoteRequest } from './types';

export interface QuoteSubmitResponse {
  message: string;
  id?: string;
  requestedQuoteId?: string;
}

/** Public quote request record (PocketBase-style collection). */
export interface RequestedQuoteRecord {
  id: string;
  collectionId: string;
  collectionName: string;
  created: string;
  updated: string;
  cargoType: string;
  weight: number;
  departurePort: string;
  departureTime: string;
  arrivalPort: string;
  arrivalTime: string;
  dimensions: string;
  additionalInfo?: string;
  fname: string;
  lname: string;
  email: string;
  phoneNumber: string;
  customer?: string;
  status?: string;
}

export async function submitQuote(data: QuoteRequest): Promise<QuoteSubmitResponse> {
  return api<QuoteSubmitResponse>('/api/quote', {
    method: 'POST',
    body: data,
  });
}

/** List public quote requests for admin review / promote-to-cargo. */
export async function listRequestedQuotes(): Promise<RequestedQuoteRecord[]> {
  return adminList<RequestedQuoteRecord>('requestedQuotes', {
    page: 1,
    perPage: 100,
    sort: '-created',
  });
}
