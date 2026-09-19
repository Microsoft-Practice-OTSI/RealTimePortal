export interface ChatMessage {
  id: number;
  conversationId: number;
  senderUserId: number;
  message: string;
  sentAt: string;
  isRead: boolean;
  readAt: string | null;
}