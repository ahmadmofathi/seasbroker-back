export interface ApiError {
  message: string;
  status: number;
  data: Record<string, unknown>;
}

export interface PaginatedResponse<T> {
  page: number;
  perPage: number;
  totalItems: number;
  totalPages: number;
  items: T[];
}

export interface PocketBaseRecord {
  id: string;
  collectionId: string;
  collectionName: string;
  created: string;
  updated: string;
}

export interface SuperuserAuthResponse {
  token: string;
  record: PocketBaseRecord & {
    email: string;
    verified: boolean;
  };
}

export interface UserAuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: {
    id: string;
    email: string;
    verified: boolean;
    roles: string[];
    created: string;
    updated: string;
  };
}

export interface QuoteRequest {
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
}

/** Tags embedded in additionalInfo so admin can tell which public form was used. */
export type PublicServiceTag =
  | 'Cargo Brokerage'
  | 'Ship Brokerage'
  | 'Customs Clearance'
  | 'Contact';

export function withServiceTag(tag: PublicServiceTag, notes?: string): string {
  const body = notes?.trim() ?? '';
  return body ? `[${tag}] ${body}` : `[${tag}]`;
}

export interface ChatTokenResponse {
  token: string;
  chatId: string;
}

export interface ChatRecord extends PocketBaseRecord {
  name: string;
}

export interface MessageRecord extends PocketBaseRecord {
  chatId: string;
  content: string;
  isAdmin: boolean;
}

export interface AnonymousMessageBody {
  token: string;
  chatId: string;
  content: string;
}

export interface AdminMessageBody {
  chatId: string;
  content: string;
}

export type CargoStatus = 'Draft' | 'Open' | 'Matched' | 'Closed' | 'Cancelled';

export interface CargoListingRecord extends PocketBaseRecord {
  customer: string;
  cargoType: string;
  weight: number;
  dimensions: string;
  departurePort: string;
  departureTime: string;
  arrivalPort: string;
  arrivalTime: string;
  requestedQuote?: string;
  referenceNumber?: string;
  status: CargoStatus;
  priority: number;
  additionalInfo?: string;
}

export interface PromoteFromQuoteBody {
  requestedQuoteId: string;
  referenceNumber?: string;
  status?: CargoStatus;
  priority?: number;
}

export type VesselStatus = 'Active' | 'Inactive' | 'Maintenance';

export interface VesselRecord extends PocketBaseRecord {
  name: string;
  imoNumber: string;
  vesselType: string;
  dwt: number;
  teuCapacity?: number | null;
  lengthOverall: number;
  beam: number;
  draft: number;
  currentPort: string;
  flagCountry: string;
  status: VesselStatus;
  customer?: string;
  notes?: string;
}

export interface VesselAvailabilityRecord extends PocketBaseRecord {
  vesselId: string;
  availableFrom: string;
  availableTo: string;
  openPort: string;
  destinationPort: string;
  isActive?: boolean;
}

export type MatchStatus =
  | 'Proposed'
  | 'PendingApproval'
  | 'Approved'
  | 'Rejected'
  | 'Expired'
  | 'Cancelled'
  | 'Completed';

export type MatchSource = 'Automatic' | 'Manual';

export interface MatchRecord extends PocketBaseRecord {
  cargoListingId: string;
  vesselId: string;
  score: number;
  status: MatchStatus;
  source: MatchSource;
  matchReason: string;
  scoreBreakdown?: string;
  expiresAt?: string;
  chatId?: string;
  rowVersion?: string;
  approvedBy?: string;
  approvedAt?: string;
}

export interface MatchingRuleRecord extends PocketBaseRecord {
  name: string;
  criterion: string;
  weight: number;
  isActive: boolean;
  configuration?: string;
}

export interface RunMatchingBody {
  cargoListingId?: string;
  vesselId?: string;
}

export interface RunMatchingResponse {
  matchesCreated: number;
  matchesSkipped: number;
  items: MatchRecord[];
}

export interface ManualMatchBody {
  cargoListingId: string;
  vesselId: string;
  score: number;
  matchReason: string;
}

export interface MatchActionBody {
  reason?: string;
  rowVersion?: string;
}

export type NotificationType =
  | 'MatchPendingApproval'
  | 'MatchApproved'
  | 'MatchRejected'
  | 'MatchCancelled'
  | 'MatchCompleted'
  | 'NewChatMessage'
  | 'SystemNotification';

export type NotificationStatus = 'Unread' | 'Read' | 'Archived';

export interface NotificationRecord {
  id: string;
  userId: string;
  title: string;
  message: string;
  notificationType: NotificationType;
  status: NotificationStatus;
  createdAt: string;
  readAt: string | null;
  payload: string;
}

// ── Dynamic Form Builder ──

export type FormFieldType =
  | 'Text'
  | 'Textarea'
  | 'Number'
  | 'Decimal'
  | 'Date'
  | 'DateTime'
  | 'Time'
  | 'Email'
  | 'Phone'
  | 'Select'
  | 'MultiSelect'
  | 'Radio'
  | 'Checkbox'
  | 'Toggle'
  | 'File'
  | 'MultiFile';

export type FormFieldWidth = 'Full' | 'Half' | 'Third';

export type FormConditionOperator =
  | 'Equals'
  | 'NotEquals'
  | 'Contains'
  | 'GreaterThan'
  | 'GreaterThanOrEqual'
  | 'LessThan'
  | 'LessThanOrEqual'
  | 'IsEmpty'
  | 'IsNotEmpty'
  | 'In'
  | 'NotIn';

export type FormConditionCombinator = 'AND' | 'OR';

export type FormVersionStatus = 'Draft' | 'Published' | 'Archived';

export interface FormFieldOption {
  value: string;
  label: string;
  order: number;
}

export interface FormFieldValidation {
  minLength?: number | null;
  maxLength?: number | null;
  min?: number | null;
  max?: number | null;
  pattern?: string | null;
  fileMaxSizeMB?: number | null;
  allowedExtensions?: string[] | null;
  minSelections?: number | null;
  maxSelections?: number | null;
}

export interface FormFieldCondition {
  sourceFieldKey: string;
  operator: FormConditionOperator;
  value?: string | null;
}

export interface FormField {
  key: string;
  label: string;
  type: FormFieldType;
  placeholder?: string | null;
  helpText?: string | null;
  required: boolean;
  visible: boolean;
  order: number;
  width: FormFieldWidth;
  isSystemField: boolean;
  systemFieldKey?: string | null;
  defaultValue?: string | null;
  options: FormFieldOption[];
  validation?: FormFieldValidation | null;
  conditionCombinator?: FormConditionCombinator | null;
  conditions: FormFieldCondition[];
}

export interface FormSection {
  key: string;
  label: string;
  order: number;
  visible: boolean;
  fields: FormField[];
}

export interface FormSchema {
  formKey: string;
  versionNumber: number;
  status: FormVersionStatus;
  sections: FormSection[];
}

export interface FormSummary {
  key: string;
  name: string;
  description?: string | null;
  publishedVersionNumber?: number | null;
  publishedAt?: string | null;
  draftVersionNumber?: number | null;
  hasUnpublishedDraft: boolean;
}

export interface SubmitFormResponse {
  submissionId: string;
  requestedQuoteId?: string | null;
}

export interface ChatEvent {
  action: string;
  record: ChatRecord;
}

export interface MessageEvent {
  action: string;
  record: MessageRecord;
}

export interface NotificationEvent {
  action: string;
  record: NotificationRecord;
}
