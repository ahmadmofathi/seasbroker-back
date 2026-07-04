import { api, createRecord } from './client';
import {
  adminCreate,
  adminGetOne,
  adminList,
} from './adminClient';
import type {
  AdminMessageBody,
  AnonymousMessageBody,
  ChatRecord,
  ChatTokenResponse,
  MessageRecord,
} from './types';

export async function getChatToken(): Promise<ChatTokenResponse> {
  return api<ChatTokenResponse>('/api/get-chat-token', { method: 'POST' });
}

export async function listChats(): Promise<ChatRecord[]> {
  return adminList<ChatRecord>('chats', { page: 1, perPage: 50 });
}

export async function getChat(id: string): Promise<ChatRecord> {
  return adminGetOne<ChatRecord>('chats', id);
}

export async function listMessages(chatId: string): Promise<MessageRecord[]> {
  return adminList<MessageRecord>('messages', {
    page: 1,
    perPage: 50,
    filter: `chatId = "${chatId}"`,
    sort: 'created',
  });
}

export async function sendAnonymousMessage(body: AnonymousMessageBody): Promise<MessageRecord> {
  return createRecord<MessageRecord>('messages', body);
}

export async function sendAdminMessage(body: AdminMessageBody): Promise<MessageRecord> {
  return adminCreate<MessageRecord>('messages', body);
}
