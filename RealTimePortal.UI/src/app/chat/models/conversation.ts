export interface Conversation {
  id: number;
  conversationType: string;
  createdAt: string;
  participantUserIds: number[];
  unreadMessageCount: number;
}