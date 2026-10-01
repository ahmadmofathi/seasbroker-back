import { adminDownloadFile, adminList } from './adminClient';
import { api } from './client';
import type { QuoteRequest } from './types';

export interface QuoteSubmitResponse {
  message: string;
  id?: string;
  requestedQuoteId?: string;
}

/** Public quote request record (PocketBase-style collection). */
export interface RequestAttachment {
  id: string;
  submissionId: string;
  fieldKey: string;
  fileName: string;
  sizeBytes: number;
}

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
  trackingNumber?: string;
  /** Files the customer uploaded with the request. */
  attachments?: RequestAttachment[];
  isPromoted?: boolean;
  /** False for Ship, Clearance and Contact requests - only cargo requests can become cargo listings. */
  canPromote?: boolean;
  cargoListingId?: string;
  cargoListingReference?: string;
  /** Ship Brokerage requests are added to the fleet as vessels instead of becoming cargo. */
  canPromoteToVessel?: boolean;
  vesselId?: string;
  vesselName?: string;
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

/** Downloads a file the customer uploaded with their request (admin only). */
export function downloadAttachment(attachment: RequestAttachment): Promise<void> {
  return adminDownloadFile(
    `/api/forms/submissions/${attachment.submissionId}/files/${attachment.id}`,
    attachment.fileName,
  );
}
