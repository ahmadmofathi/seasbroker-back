import { useCallback, useEffect, useRef, useState } from "react";
import Cookies from 'js-cookie';
import { recordToChatMessage, type ChatMessage } from "../../types/chatMessages";
import { chatApi, signalrApi } from '../../api';
import { SeasBrokerApiError } from '../../api/client';
import type * as signalR from '@microsoft/signalr';

const COOKIE_DAYS = 7; // matches the server: a visitor's chat stays reachable for a week
const LAST_SEEN_KEY = 'seasbroker.chat.lastSeen';

function readLastSeen(): Date | null {
  try {
    const raw = localStorage.getItem(LAST_SEEN_KEY);
    const date = raw ? new Date(raw) : null;
    return date && !Number.isNaN(date.getTime()) ? date : null;
  } catch {
    return null;
  }
}

function writeLastSeen(date: Date): void {
  try {
    localStorage.setItem(LAST_SEEN_KEY, date.toISOString());
  } catch {
    // storage unavailable (private window): the unread count just won't survive a reload
  }
}

/** A short two-tone chime, so a reply is noticed even when the visitor is looking elsewhere. */
function playChime(): void {
  try {
    const Ctx = window.AudioContext;
    const ctx = new Ctx();
    const now = ctx.currentTime;
    [660, 880].forEach((frequency, i) => {
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.type = 'sine';
      osc.frequency.value = frequency;
      gain.gain.setValueAtTime(0.0001, now + i * 0.18);
      gain.gain.exponentialRampToValueAtTime(0.25, now + i * 0.18 + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, now + i * 0.18 + 0.3);
      osc.connect(gain).connect(ctx.destination);
      osc.start(now + i * 0.18);
      osc.stop(now + i * 0.18 + 0.32);
    });
    window.setTimeout(() => { void ctx.close(); }, 900);
  } catch {
    // browsers block sound until the visitor has interacted with the page - that's fine
  }
}

interface ReplyToast {
  id: string;
  senderName: string;
  content: string;
  created: Date;
}

const Chat: React.FC = () => {

  const [chatWidgetVisible, setChatWidgetVisible] = useState(false);
  const [message, setMessage] = useState('');
  const [chatId, setChatId] = useState('');
  const [token, setToken] = useState('');
  const [chat, setChat] = useState<ChatMessage[]>([]);
  const [lastSeen, setLastSeen] = useState<Date | null>(() => readLastSeen());
  const [replyToast, setReplyToast] = useState<ReplyToast | null>(null);
  const toastTimerRef = useRef<number | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const widgetOpenRef = useRef(false);
  const endRef = useRef<HTMLDivElement | null>(null);
  const baseTitle = useRef(document.title);

  widgetOpenRef.current = chatWidgetVisible;

  // Replies the visitor hasn't read yet: ours that arrived after they last looked at the conversation.
  const unread = chatWidgetVisible
    ? 0
    : chat.filter((m) => m.isAdmin && (!lastSeen || m.created > lastSeen)).length;

  const dismissToast = useCallback((e?: React.MouseEvent) => {
    if (e) e.stopPropagation();
    if (toastTimerRef.current) {
      window.clearTimeout(toastTimerRef.current);
      toastTimerRef.current = null;
    }
    setReplyToast(null);
  }, []);

  const showToast = useCallback((msg: ChatMessage) => {
    if (toastTimerRef.current) {
      window.clearTimeout(toastTimerRef.current);
    }
    setReplyToast({
      id: msg.id,
      senderName: 'Customer Support',
      content: msg.content,
      created: msg.created,
    });
    // Auto dismiss toast after 12 seconds
    toastTimerRef.current = window.setTimeout(() => {
      setReplyToast(null);
      toastTimerRef.current = null;
    }, 12000);
  }, []);

  const createToken = useCallback(() => {
    chatApi.getChatToken().then(data => {
      if (data.chatId) {
        Cookies.set('chatId', data.chatId, { expires: COOKIE_DAYS });
        Cookies.set('chatToken', data.token, { expires: COOKIE_DAYS });

        setChatId(data.chatId);
        setToken(data.token);
      }
    }).catch((error: unknown) => {
      console.error('Error getting token:', error);
    });
  }, []);

  /** Tells the visitor about a new reply with sound, in-app toast notification, and browser notification. */
  const notifyReply = useCallback((msg: ChatMessage) => {
    playChime();

    // Show floating in-app notification bubble if chat is not open
    if (!widgetOpenRef.current) {
      showToast(msg);
    }

    // Gentle vibration on mobile devices if supported
    if (typeof navigator !== 'undefined' && 'vibrate' in navigator) {
      try { navigator.vibrate(200); } catch { /* ignore */ }
    }

    // System browser notification if granted
    if ('Notification' in window && Notification.permission === 'granted') {
      try {
        const shown = new Notification('New reply from Customer Support', {
          body: msg.content,
          tag: 'seasbroker-chat',
        });
        shown.onclick = () => {
          window.focus();
          setChatWidgetVisible(true);
          dismissToast();
          shown.close();
        };
      } catch {
        /* ignore */
      }
    }
  }, [showToast, dismissToast]);

  const toggleChatWidget = () => {
    const next = !chatWidgetVisible;
    setChatWidgetVisible(next);
    if (next) {
      dismissToast();
    }
  };

  const openChatFromToast = () => {
    dismissToast();
    setChatWidgetVisible(true);
  };

  useEffect(() => {
    const cookieChatId = Cookies.get('chatId');
    const cookieToken = Cookies.get('chatToken');
    if (!cookieChatId || !cookieToken) {
      createToken();
    } else {
      setChatId(cookieChatId);
      setToken(cookieToken);
    }
  }, [createToken]);

  // Bring back the conversation, including replies sent while the visitor was away.
  useEffect(() => {
    if (!chatId || !token) return;
    let active = true;
    chatApi.getChatHistory(chatId, token)
      .then((records) => {
        if (!active) return;
        const history = records.map(recordToChatMessage);
        setChat((live) => {
          const ids = new Set(history.map((m) => m.id));
          return [...history, ...live.filter((m) => !ids.has(m.id))];
        });

        const storedLastSeen = readLastSeen();
        // First time on this device: only flag replies to the visitor's latest message, not the whole past.
        if (!storedLastSeen) {
          const own = history.filter((m) => !m.isAdmin);
          if (own.length > 0) setLastSeen(own[own.length - 1].created);
        }

        // If there are unread admin replies waiting, show the in-app notification popup
        const unreadAdmin = history.filter((m) => m.isAdmin && (!storedLastSeen || m.created > storedLastSeen));
        if (unreadAdmin.length > 0 && !widgetOpenRef.current) {
          const latest = unreadAdmin[unreadAdmin.length - 1];
          showToast(latest);
        }
      })
      .catch((error: unknown) => {
        if (!active) return;
        // The chat token expired or is no longer valid: start a fresh conversation.
        if (error instanceof SeasBrokerApiError && (error.status === 400 || error.status === 401)) {
          Cookies.remove('chatId');
          Cookies.remove('chatToken');
          setChat([]);
          setChatId('');
          setToken('');
          createToken();
        } else {
          console.error('Error loading chat history:', error);
        }
      });
    return () => {
      active = false;
    };
  }, [chatId, token, createToken, showToast]);

  useEffect(() => {
    if (!chatId || !token) return;

    // Live updates come from SignalR.
    const connection = signalrApi.createChatHubConnection();
    connectionRef.current = connection;

    signalrApi.onMessageEvent(connection, (event) => {
      if (event.record.chatId === chatId) {
        const msg = recordToChatMessage(event.record);
        if (event.action === 'create') {
          setChat((prev) => (prev.some((m) => m.id === msg.id) ? prev : [...prev, msg]));
          if (msg.isAdmin) notifyReply(msg);
        } else if (event.action === 'update') {
          setChat((prev) => prev.map((m) => (m.id === msg.id ? msg : m)));
        } else if (event.action === 'delete') {
          setChat((prev) => prev.filter((m) => m.id !== msg.id));
        }
      }
    });

    connection.start()
      .then(() => signalrApi.joinChat(connection, chatId, token))
      .catch((error: unknown) => {
        console.error('SignalR connection error:', error);
      });

    return () => {
      void connection.stop();
      connectionRef.current = null;
    };
  }, [chatId, token, notifyReply]);

  // Looking at the conversation counts as reading it.
  useEffect(() => {
    if (!chatWidgetVisible || chat.length === 0) return;
    const newest = chat.reduce((latest, m) => (m.created > latest ? m.created : latest), chat[0].created);
    setLastSeen(newest);
    writeLastSeen(newest);
    endRef.current?.scrollIntoView({ block: 'end' });
  }, [chatWidgetVisible, chat]);

  // Tab title: "(1) New reply" while a reply is waiting and the visitor is on another tab.
  useEffect(() => {
    const title = baseTitle.current;
    const update = () => {
      document.title = unread > 0 && document.hidden ? `(${String(unread)}) New reply - ${title}` : title;
    };
    update();
    document.addEventListener('visibilitychange', update);
    return () => {
      document.removeEventListener('visibilitychange', update);
      document.title = title;
    };
  }, [unread]);

  const sendMessage = (e: React.FormEvent) => {
    e.preventDefault();
    if (message.trim() === '' || !chatId || !token) return;

    // Asked once, right after the visitor's own action, so the browser will show the prompt.
    if ('Notification' in window && Notification.permission === 'default') {
      void Notification.requestPermission();
    }

    chatApi.sendAnonymousMessage({
      token,
      chatId,
      content: message,
    }).then((record) => {
      setMessage('');
      setChat((prev) => {
        if (prev.some((m) => m.id === record.id)) return prev;
        return [...prev, recordToChatMessage(record)];
      });
    }).catch((err: unknown) => {
      console.log("Failed to send message: " + String(err));
    });
  };

  const hasSentMessage = chat.some((m) => !m.isAdmin);

  return (
    <>
      {/* Floating In-App Reply Toast Notification */}
      {!chatWidgetVisible && replyToast && (
        <div
          role="alert"
          aria-live="polite"
          className="chat-reply-toast flex items-start gap-3"
          onClick={openChatFromToast}
        >
          <div className="relative shrink-0 mt-0.5">
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-red-600 to-red-800 text-white flex items-center justify-center shadow-md">
              <i className="ri-customer-service-2-fill text-xl" />
            </div>
            <span className="absolute bottom-0 right-0 w-3 h-3 bg-emerald-500 border-2 border-white rounded-full animate-pulse" />
          </div>

          <div className="flex-1 min-w-0">
            <div className="flex items-center justify-between gap-1 mb-1">
              <span className="text-sm font-bold text-gray-900 truncate">
                Customer Support
              </span>
              <span className="text-[11px] text-gray-400 shrink-0 font-medium">
                Just now
              </span>
            </div>
            <p className="text-xs text-gray-700 leading-snug line-clamp-2 break-words">
              {replyToast.content}
            </p>
            <div className="flex items-center gap-1 mt-2 text-[11px] font-semibold text-red-600 hover:text-red-700">
              <span>Click to open & reply</span>
              <i className="ri-arrow-right-line text-xs" />
            </div>
          </div>

          <button
            type="button"
            className="shrink-0 text-gray-400 hover:text-gray-700 p-1 -mr-1 -mt-1 rounded-full hover:bg-gray-100 transition-colors"
            onClick={dismissToast}
            aria-label="Dismiss notification"
          >
            <i className="ri-close-line text-base" />
          </button>
        </div>
      )}

      {/* Floating Action Button (FAB) */}
      <button
        type="button"
        id="chat-widget"
        className={`chat-fab${unread > 0 ? ' has-unread' : ''}`}
        aria-label={unread > 0 ? `Chat - ${String(unread)} new ${unread === 1 ? 'reply' : 'replies'}` : 'Chat with us'}
        onClick={toggleChatWidget}
      >
        <span className="chat-fab-pill">
          {unread > 0 ? `${unread} new ${unread === 1 ? 'reply' : 'replies'}` : 'Chat with us'}
        </span>
        <i className={chatWidgetVisible ? 'ri-close-line' : 'ri-chat-4-fill'} />
        {unread > 0 && <span className="chat-fab-badge">{unread}</span>}
      </button>

      {/* Chat Window */}
      {chatWidgetVisible && (
        <div className="fixed bottom-[104px] right-6 bg-white rounded-2xl shadow-2xl border border-gray-100 w-[380px] max-w-[calc(100vw-2.5rem)] h-[520px] max-h-[calc(100vh-130px)] flex flex-col justify-between z-[1050] overflow-hidden animate-in fade-in zoom-in-95 duration-200">
          {/* Header */}
          <div className="p-4 bg-gradient-to-r from-red-600 to-red-700 text-white flex items-center justify-between shadow-sm">
            <div className="flex items-center gap-3">
              <div className="relative">
                <div className="w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center text-white">
                  <i className="ri-customer-service-2-fill text-xl" />
                </div>
                <span className="absolute bottom-0 right-0 w-2.5 h-2.5 bg-emerald-400 border-2 border-red-700 rounded-full" />
              </div>
              <div className="flex flex-col">
                <span className="text-sm font-bold leading-tight">Customer Support</span>
                <span className="text-xs text-white/80">Online & Ready to Help</span>
              </div>
            </div>
            <button
              type="button"
              onClick={() => setChatWidgetVisible(false)}
              className="text-white/80 hover:text-white p-1 rounded-full hover:bg-white/10 transition-colors"
              aria-label="Close chat"
            >
              <i className="ri-close-line text-2xl" />
            </button>
          </div>

          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto p-4 bg-gray-50/50">
            <div className="flex flex-col gap-2.5">
              {chat.length === 0 && (
                <div className="text-center py-8 text-gray-400 text-xs">
                  <i className="ri-chat-smile-2-line text-3xl mb-1 block" />
                  Say hello! How can we assist you today?
                </div>
              )}
              {chat.map((msg) => (
                <div
                  key={msg.id}
                  className={`p-3 max-w-[85%] text-sm rounded-2xl shadow-xs ${
                    !msg.isAdmin
                      ? 'bg-red-600 text-white ml-auto rounded-br-xs'
                      : 'bg-white text-gray-800 border border-gray-100 rounded-bl-xs'
                  }`}
                >
                  <span className="break-words leading-relaxed">{msg.content}</span>
                  <div
                    className={`text-[10px] mt-1.5 flex items-center gap-1 ${
                      !msg.isAdmin ? 'text-red-100 justify-end' : 'text-gray-400'
                    }`}
                  >
                    <span>{msg.created.toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}</span>
                  </div>
                </div>
              ))}
              <div ref={endRef} />
            </div>
          </div>

          {/* Note */}
          {hasSentMessage && (
            <div className="px-4 py-1.5 bg-gray-50 border-t border-gray-100">
              <p className="text-[11px] text-gray-500 leading-tight">
                💡 We'll alert you with a notification when we reply.
              </p>
            </div>
          )}

          {/* Input Form */}
          <form className="p-3 bg-white border-t border-gray-100 flex items-center gap-2" onSubmit={sendMessage}>
            <input
              value={message}
              onChange={(e) => { setMessage(e.target.value); }}
              type="text"
              placeholder="Type your message..."
              className="flex-1 px-3.5 py-2.5 text-sm bg-gray-50 border border-gray-200 rounded-xl focus:outline-none focus:border-red-500 focus:bg-white transition-all"
            />
            <button
              type="submit"
              disabled={!message.trim()}
              className="w-10 h-10 rounded-xl bg-red-600 hover:bg-red-700 disabled:opacity-40 disabled:cursor-not-allowed text-white flex items-center justify-center transition-colors shadow-sm shrink-0"
              aria-label="Send message"
            >
              <i className="ri-send-plane-fill text-lg" />
            </button>
          </form>
        </div>
      )}
    </>
  );
};

export default Chat;
