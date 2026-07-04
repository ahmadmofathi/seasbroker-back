export function recordToChatMessage(record: {
  id: string;
  chatId: string;
  content: string;
  isAdmin: boolean;
  created: string;
  updated: string;
}): ChatMessage {
  return {
    id: record.id,
    chatId: record.chatId,
    content: record.content,
    isAdmin: record.isAdmin,
    created: new Date(record.created),
    updated: new Date(record.updated),
  };
}

export interface ChatMessage {
  id: string;
  chatId: string;
  content: string;
  isAdmin: boolean;
  created: Date;
  updated: Date;
}
