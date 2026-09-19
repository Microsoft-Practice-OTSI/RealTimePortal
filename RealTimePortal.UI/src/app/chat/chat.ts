import {
  Component,
  OnDestroy,
  OnInit,
  signal
} from '@angular/core';

import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { ChatApi } from './chat-api';
import { Auth } from '../auth/auth';
import { Realtime } from '../shared/services/realtime';

import { ChatUser } from './models/chat-user';
import { Conversation } from './models/conversation';
import { ChatMessage } from './models/chat-message';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    FormsModule,
    DatePipe,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './chat.html',
  styleUrl: './chat.css'
})
export class Chat
  implements OnInit, OnDestroy {

  readonly users =
    signal<ChatUser[]>([]);

  readonly conversations =
    signal<Conversation[]>([]);

  readonly messages =
    signal<ChatMessage[]>([]);

  readonly selectedUser =
    signal<ChatUser | null>(null);

  readonly selectedConversation =
    signal<Conversation | null>(null);

  readonly messageText =
    signal('');

  readonly currentUserId =
    signal<number | null>(null);

  readonly loading =
    signal(false);

  constructor(
    private readonly chatApi: ChatApi,
    private readonly auth: Auth,
    private readonly realtime: Realtime
  ) {
  }

  ngOnInit(): void {

    this.currentUserId.set(
      this.auth.getCurrentUserId()
    );

    this.loadUsers();

    this.loadConversations();

    this.realtime.registerChatMessageHandler(
      message => {
        this.handleIncomingMessage(
          message as ChatMessage
        );
      }
    );


    this.realtime.registerChatUnreadMessageHandler(
      message => {
        this.handleUnreadMessage(
          message as ChatMessage
        );
      }
    );

  }

  private loadUsers(): void {

    this.chatApi
      .getUsers()
      .subscribe({
        next: users => {
          this.users.set(users);
        },
        error: error => {
          console.error(
            'Failed to load chat users.',
            error
          );
        }
      });
  }

  private loadConversations(): void {

    this.chatApi
      .getConversations()
      .subscribe({
        next: conversations => {

          this.conversations.set(
            conversations
          );
        },
        error: error => {

          console.error(
            'Failed to load conversations.',
            error
          );
        }
      });
  }

  selectUser(user: ChatUser): void {

    this.selectedUser.set(user);

    const currentUserId =
      this.currentUserId();

    if (currentUserId === null) {
      return;
    }

    const existingConversation =
      this.conversations().find(
        conversation =>
          conversation.participantUserIds.includes(
            user.id
          )
      );

    if (existingConversation) {

      this.openConversation(
        existingConversation
      );

      return;
    }

    this.createConversation(user);
  }

  private createConversation(
    user: ChatUser
  ): void {

    const currentUserId =
      this.currentUserId();

    if (currentUserId === null) {
      return;
    }

    this.chatApi
      .createConversation([
        currentUserId,
        user.id
      ])
      .subscribe({
        next: conversation => {

          this.conversations.update(
            conversations => [
              ...conversations,
              conversation
            ]
          );

          this.openConversation(
            conversation
          );
        },
        error: error => {

          console.error(
            'Failed to create conversation.',
            error
          );
        }
      });
  }

  private openConversation(
    conversation: Conversation
  ): void {

    const currentConversation =
      this.selectedConversation();

    if (
      currentConversation &&
      currentConversation.id !==
        conversation.id
    ) {
      this.realtime.leaveConversation(
        currentConversation.id
      );
    }

    /*
     * Set the selected conversation.
     */
    this.selectedConversation.set(
      conversation
    );

    /*
     * Clear the current messages
     * while loading the selected conversation.
     */
    this.messages.set([]);

    /*
     * Join the SignalR conversation group.
     */
    this.realtime.joinConversation(
      conversation.id
    );

    /*
     * Load messages from the API.
     */
    this.loadMessages(
      conversation.id
    );

    /*
     * Mark messages as read in the database.
     */
    this.chatApi
      .markAsRead(conversation.id)
      .subscribe({
        next: () => {

          console.log(
            `Conversation ${conversation.id} marked as read.`
          );
        },
        error: error => {

          console.error(
            'Failed to mark messages as read.',
            error
          );
        }
      });

    /*
     * Immediately clear the unread badge
     * in the Angular UI.
     *
     * The database is updated by markAsRead().
     */
    this.conversations.update(
      conversations =>
        conversations.map(item =>
          item.id === conversation.id
            ? {
                ...item,
                unreadMessageCount: 0
              }
            : item
        )
    );
  }

  getUnreadCount(userId: number): number {

    const currentUserId =
      this.currentUserId();

    if (currentUserId === null) {
      return 0;
    }

    const conversation =
      this.conversations().find(
        item =>
          item.participantUserIds.includes(
            currentUserId
          ) &&
          item.participantUserIds.includes(
            userId
          )
      );

    return conversation?.unreadMessageCount ?? 0;
  }

  getTotalUnreadCount(): number {
  return this.conversations()
    .reduce(
      (total, conversation) =>
        total + (conversation.unreadMessageCount ?? 0),
      0
    );
}

  private loadMessages(
    conversationId: number
  ): void {

    this.loading.set(true);

    this.chatApi
      .getMessages(conversationId)
      .subscribe({
        next: messages => {

          this.messages.set(
            messages
          );

          this.loading.set(false);
        },
        error: error => {

          console.error(
            'Failed to load messages.',
            error
          );

          this.loading.set(false);
        }
      });
  }

  sendMessage(): void {

    const conversation =
      this.selectedConversation();

    const message =
      this.messageText().trim();

    if (
      !conversation ||
      !message
    ) {
      return;
    }

    this.chatApi
      .sendMessage(
        conversation.id,
        message
      )
      .subscribe({
        next: sentMessage => {

          this.addMessage(
            sentMessage
          );

          this.messageText.set('');
        },
        error: error => {

          console.error(
            'Failed to send message.',
            error
          );
        }
      });
  }

  onMessageKeyDown(
    event: KeyboardEvent
  ): void {

    if (
      event.key === 'Enter' &&
      !event.shiftKey
    ) {
      event.preventDefault();

      this.sendMessage();
    }
  }

  private handleIncomingMessage(
    message: ChatMessage
  ): void {

    const currentUserId =
      this.currentUserId();

    /*
     * Ignore messages sent by the current user.
     */
    if (
      currentUserId !== null &&
      message.senderUserId === currentUserId
    ) {
      return;
    }

    const conversation =
      this.selectedConversation();

    /*
     * If this message belongs to the
     * currently opened conversation,
     * add it directly to the message list.
     */
    if (
      conversation &&
      message.conversationId ===
        conversation.id
    ) {

      this.addMessage(
        message
      );

      /*
       * Since the user is currently viewing
       * this conversation, keep it read.
       */
      this.chatApi
        .markAsRead(
          message.conversationId
        )
        .subscribe({
          error: error => {

            console.error(
              'Failed to mark incoming message as read.',
              error
            );
          }
        });

      return;
    }

    /*
     * Message belongs to another conversation.
     *
     * Increase its unread count.
     */
    this.incrementUnreadCount(
      message.conversationId
    );
  }

  private incrementUnreadCount(
    conversationId: number
  ): void {

    this.conversations.update(
      conversations =>
        conversations.map(conversation => {

          if (
            conversation.id !==
            conversationId
          ) {
            return conversation;
          }

          return {
            ...conversation,
            unreadMessageCount:
              conversation.unreadMessageCount + 1
          };
        })
    );
  }

private handleUnreadMessage(
  message: ChatMessage
): void {

  const currentUserId =
    this.currentUserId();

  if (currentUserId === null) {
    return;
  }

  if (
    message.senderUserId ===
    currentUserId
  ) {
    return;
  }

  const selectedConversation =
    this.selectedConversation();

  if (
    selectedConversation &&
    selectedConversation.id ===
      message.conversationId
  ) {
    return;
  }

  const conversationExists =
    this.conversations().some(
      conversation =>
        conversation.id ===
        message.conversationId
    );

  if (conversationExists) {

    this.incrementUnreadCount(
      message.conversationId
    );

    return;
  }

  // Conversation is not currently
  // available in the local list.
  this.loadConversations();
}
  private addMessage(
    message: ChatMessage
  ): void {

    const exists =
      this.messages().some(
        item =>
          item.id === message.id
      );

    if (exists) {
      return;
    }

    this.messages.update(
      messages => [
        ...messages,
        message
      ]
    );
  }

  isOwnMessage(
    message: ChatMessage
  ): boolean {

    return (
      message.senderUserId ===
      this.currentUserId()
    );
  }

  getSelectedUserName(): string {

    const user =
      this.selectedUser();

    if (!user) {
      return 'Select a user';
    }

    return (
      user.displayName ||
      user.userName
    );
  }

  ngOnDestroy(): void {

    const conversation =
      this.selectedConversation();

    if (conversation) {

      this.realtime.leaveConversation(
        conversation.id
      );
    }
  }
}