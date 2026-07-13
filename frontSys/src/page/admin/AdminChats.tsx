import { useEffect, useState } from 'react';
import { chatApi } from '../../api';
import { formatApiError } from '../../utils/formatApiError';
import { useAlert } from '../../context/AlertContext';
import type { ChatRecord, MessageRecord } from '../../api/types';

const AdminChats: React.FC = () => {
  const [chats, setChats] = useState<ChatRecord[]>([]);
  const [selectedChat, setSelectedChat] = useState<ChatRecord | null>(null);
  const [messages, setMessages] = useState<MessageRecord[]>([]);
  const [reply, setReply] = useState('');
  const [loading, setLoading] = useState(true);
  const { error: showError } = useAlert();

  useEffect(() => {
    chatApi.listChats()
      .then(setChats)
      .catch((e: unknown) => showError(formatApiError(e)))
      .finally(() => setLoading(false));
  }, []);

  const loadMessages = (chat: ChatRecord) => {
    setSelectedChat(chat);
    chatApi.listMessages(chat.id)
      .then(setMessages)
      .catch((e: unknown) => showError(formatApiError(e)));
  };

  const sendReply = async () => {
    if (!selectedChat || !reply.trim()) return;
    try {
      await chatApi.sendAdminMessage({ chatId: selectedChat.id, content: reply });
      setReply('');
      const updated = await chatApi.listMessages(selectedChat.id);
      setMessages(updated);
    } catch (e) {
      showError(formatApiError(e));
    }
  };

  if (loading) {
    return <div className="admin-loading"><div className="admin-spinner" /> Loading chats…</div>;
  }

  return (
    <>
      <div className="admin-chat-grid">
        <div className="admin-panel" style={{ marginBottom: 0 }}>
          <div className="admin-panel-header">
            <h2>All Chats ({chats.length})</h2>
          </div>
          <div className="admin-panel-body no-pad admin-chat-list">
            {chats.map((chat) => (
              <button
                key={chat.id}
                type="button"
                onClick={() => loadMessages(chat)}
                className={`admin-chat-item${selectedChat?.id === chat.id ? ' active' : ''}`}
              >
                <div className="name">{chat.name || 'Unnamed chat'}</div>
              </button>
            ))}
            {chats.length === 0 && (
              <div className="admin-empty">
                <i className="ri-chat-off-line" />
                No chats yet
              </div>
            )}
          </div>
        </div>

        <div className="admin-panel" style={{ marginBottom: 0 }}>
          {selectedChat ? (
            <>
              <div className="admin-panel-header">
                <h2>{selectedChat.name}</h2>
              </div>
              <div className="admin-messages">
                <div className="admin-messages-list">
                  {messages.map((msg) => (
                    <div key={msg.id} className={`admin-msg ${msg.isAdmin ? 'admin' : 'user'}`}>
                      {msg.content}
                    </div>
                  ))}
                  {messages.length === 0 && (
                    <div className="admin-empty"><p>No messages in this chat</p></div>
                  )}
                </div>
                <div className="admin-msg-input">
                  <input
                    className="admin-input"
                    value={reply}
                    onChange={(e) => setReply(e.target.value)}
                    placeholder="Reply as admin…"
                    onKeyDown={(e) => { if (e.key === 'Enter') void sendReply(); }}
                  />
                  <button type="button" className="admin-btn-sm primary" onClick={() => void sendReply()}>
                    <i className="ri-send-plane-fill" /> Send
                  </button>
                </div>
              </div>
            </>
          ) : (
            <div className="admin-empty" style={{ minHeight: 300 }}>
              <i className="ri-chat-smile-2-line" />
              Select a chat to view messages and reply
            </div>
          )}
        </div>
      </div>
    </>
  );
};

export default AdminChats;
