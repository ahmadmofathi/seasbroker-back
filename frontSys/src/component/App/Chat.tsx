import { useEffect, useRef, useState } from "react";
import profileImage from '../../assets/img/chat/Dr.Ahmed.jpg';
import Cookies from 'js-cookie';
import { recordToChatMessage, type ChatMessage } from "../../types/chatMessages";
import { chatApi, signalrApi } from '../../api';
import type * as signalR from '@microsoft/signalr';

const Chat: React.FC = () => {

  const [chatWidgetVisible, setChatWidgetVisible] = useState(false);
  const [message, setMessage] = useState('');
  const [chatId, setChatId] = useState('');
  const [token, setToken] = useState('');
  const [chat, setChat] = useState<ChatMessage[]>([]);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const createToken = () => {
    chatApi.getChatToken().then(data => {
      if (data.chatId) {
        Cookies.set('chatId', data.chatId, { expires: 1 });
        Cookies.set('chatToken', data.token, { expires: 1 });

        setChatId(data.chatId);
        setToken(data.token);
      }
    }).catch((error: unknown) => {
      console.error('Error getting token:', error);
    });
  };

  useEffect(() => {
    if (!chatId || !token) return;

    // Anonymous visitors cannot list messages via admin APIs — live updates come from SignalR.
    const connection = signalrApi.createChatHubConnection();
    connectionRef.current = connection;

    signalrApi.onMessageEvent(connection, (event) => {
      if (event.record.chatId === chatId) {
        const msg = recordToChatMessage(event.record);
        if (event.action === 'create') {
          setChat((prev) => [...prev, msg]);
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
  }, [chatId, token]);

  useEffect(() => {
    const cookieChatId = Cookies.get('chatId');
    const cookieToken = Cookies.get('chatToken');
    if (!cookieChatId || !cookieToken) {
      createToken();
    } else {
      setChatId(cookieChatId);
      setToken(cookieToken);
    }
  }, []);

  const sendMessage = (e: React.FormEvent) => {
    e.preventDefault();
    if (message.trim() === '' || !chatId || !token) return;
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
  }

  return (
    <>
      <div onClick={() => { setChatWidgetVisible(!chatWidgetVisible) }} id='chat-widget' className='text-lg bottom-4 right-4 fixed z-50 size-16 bg-white p-4 rounded-full shadow-xl cursor-pointer'>
        <i
          className="ri-chat-4-fill ri-xl absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 transition-opacity duration-300"
          style={{
            opacity: chatWidgetVisible ? 0 : 1,
            pointerEvents: chatWidgetVisible ? 'none' : 'auto',
          }}
        />
        <i
          className="ri-close-fill ri-xl absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 transition-opacity duration-300"
          style={{
            opacity: chatWidgetVisible ? 1 : 0,
            pointerEvents: chatWidgetVisible ? 'auto' : 'none',
          }}
        />
      </div>

      {
        chatWidgetVisible && (
          <div className='fixed bottom-32 right-4 bg-white p-6 rounded-lg shadow-lg w-112 h-[50%] flex flex-col justify-between z-50'>
            <div className="flex flex-row">
              <img
                src={profileImage}
                alt="Dr. Ahmed Samir Shehata"
                className="w-12 h-12 rounded-full mr-4"
              />
              <div className='flex flex-col'>
                <span className='text-sm text-gray-500'>You are chatting with</span>
                <span className='text-lg font-medium'>Customer Service</span>
              </div>
            </div>
            <div className='flex-1 overflow-y-auto mt-4'>
              <div className='flex flex-col gap-2'>
                {chat.map((msg) => (
                  <div
                    key={msg.id}
                    className={`p-2 w-[80%] ${!msg.isAdmin ? "bg-green-300 ml-auto " : "bg-gray-300"} rounded-lg`}>
                    <span>{msg.content}</span>
                  </div>
                ))}
              </div>
            </div>
            <form className='flex flex-row bottom-0' onSubmit={sendMessage}>
              <input
                value={message}
                onChange={(e) => {setMessage(e.target.value);}}
                type="text"
                placeholder="Type your message..."
                className='w-full p-2 border border-gray-300 rounded'
              />
              <button type="submit" className='w-[15%] p-2 rounded hover:border hover:border-gray-300 active:border-black active:bg-gray-300 content-center'>
                <i className="ri-send-plane-fill ri-xl " />
              </button>
            </form>
          </div >
        )
      }
    </>
  );
};

export default Chat;
