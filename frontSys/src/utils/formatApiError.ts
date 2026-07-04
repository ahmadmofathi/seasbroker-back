import { SeasBrokerApiError } from '../api/client';
import { ClientResponseError } from 'pocketbase';

export function formatApiError(error: unknown): string {
  if (error instanceof SeasBrokerApiError) {
    switch (error.status) {
      case 401:
        if (error.message && !error.message.startsWith('HTTP')) {
          return error.message;
        }
        return 'Could not load data. Try signing out and signing in again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 404:
        return 'The requested data was not found.';
      case 409:
        return error.message || 'This action conflicts with the current state.';
      default:
        if (error.message && !error.message.startsWith('HTTP')) {
          return error.message;
        }
        return 'Something went wrong. Please try again.';
    }
  }
  if (error instanceof ClientResponseError) {
    if (error.status === 401) return 'Your session has expired. Please sign in again.';
    return error.message || 'Something went wrong. Please try again.';
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'An unexpected error occurred.';
}
