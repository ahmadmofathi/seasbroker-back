import * as signalR from '@microsoft/signalr';
import { resolveApiOrigin } from './client';
import type { ChatEvent, MessageEvent, NotificationEvent } from './types';

export function createChatHubConnection(accessToken?: string): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${resolveApiOrigin()}/hubs/chat`, {
      accessTokenFactory: accessToken ? () => accessToken : undefined,
    })
    .withAutomaticReconnect()
    .build();
}

export function createNotificationsHubConnection(accessToken: string): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${resolveApiOrigin()}/hubs/notifications`, {
      accessTokenFactory: () => accessToken,
    })
    .withAutomaticReconnect()
    .build();
}

export async function joinAdminChat(connection: signalR.HubConnection): Promise<void> {
  await connection.invoke('JoinAdmin');
}

export async function joinChat(
  connection: signalR.HubConnection,
  chatId: string,
  chatToken: string,
): Promise<void> {
  await connection.invoke('JoinChat', chatId, chatToken);
}

export async function joinUserNotifications(connection: signalR.HubConnection): Promise<void> {
  await connection.invoke('JoinUser');
}

export async function joinAdminNotifications(connection: signalR.HubConnection): Promise<void> {
  await connection.invoke('JoinAdmin');
}

export function onChatEvent(
  connection: signalR.HubConnection,
  handler: (event: ChatEvent) => void,
): void {
  connection.on('ReceiveChatEvent', handler);
}

export function onMessageEvent(
  connection: signalR.HubConnection,
  handler: (event: MessageEvent) => void,
): void {
  connection.on('ReceiveMessageEvent', handler);
}

export function onNotificationEvent(
  connection: signalR.HubConnection,
  handler: (event: NotificationEvent) => void,
): void {
  connection.on('ReceiveNotification', handler);
}
