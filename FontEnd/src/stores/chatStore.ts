import { create } from 'zustand';

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderId: string;
  content: string;
  messageType: 'TEXT' | 'IMAGE';
  isRead: boolean;
  createdAt: string;
}

interface ChatStore {
  messages: ChatMessage[];
  unreadCount: number;
  
  setMessages: (messages: ChatMessage[]) => void;
  addMessage: (message: ChatMessage) => void;
  incrementUnread: () => void;
  resetUnread: () => void;
  clearMessages: () => void;
}

export const useChatStore = create<ChatStore>((set) => ({
  messages: [],
  unreadCount: 0,
  
  setMessages: (messages) => set({ messages }),
  
  addMessage: (message) => set((state) => {
    // Prevent duplicate messages
    if (state.messages.some(m => m.id === message.id)) {
      return state;
    }
    return { messages: [...state.messages, message] };
  }),
  
  incrementUnread: () => set((state) => ({ unreadCount: state.unreadCount + 1 })),
  
  resetUnread: () => set({ unreadCount: 0 }),
  
  clearMessages: () => set({ messages: [], unreadCount: 0 })
}));
