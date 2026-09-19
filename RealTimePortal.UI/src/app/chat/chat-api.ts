import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';

import { ChatUser } from './models/chat-user';
import { Conversation } from './models/conversation';
import { ChatMessage } from './models/chat-message';

@Injectable({
  providedIn: 'root'
})
export class ChatApi {

  private readonly baseUrl =
    `${environment.apiUrl}/chat`;

  constructor(
    private readonly httpClient: HttpClient
  ) {
  }

  getUsers(): Observable<ChatUser[]> {
    return this.httpClient.get<ChatUser[]>(
      `${this.baseUrl}/users`
    );
  }

  getConversations(): Observable<Conversation[]> {
    return this.httpClient.get<Conversation[]>(
      `${this.baseUrl}/conversations`
    );
  }

  createConversation(
    participantUserIds: number[]
  ): Observable<Conversation> {

    return this.httpClient.post<Conversation>(
      `${this.baseUrl}/conversations`,
      {
        conversationType: 'Direct',
        participantUserIds
      }
    );
  }

  getMessages(
    conversationId: number
  ): Observable<ChatMessage[]> {

    return this.httpClient.get<ChatMessage[]>(
      `${this.baseUrl}/conversations/${conversationId}/messages`
    );
  }

  sendMessage(
    conversationId: number,
    message: string
  ): Observable<ChatMessage> {

    return this.httpClient.post<ChatMessage>(
      `${this.baseUrl}/conversations/${conversationId}/messages`,
      {
        message
      }
    );
  }

  markAsRead(
    conversationId: number
  ): Observable<void> {

    return this.httpClient.post<void>(
      `${this.baseUrl}/conversations/${conversationId}/read`,
      {}
    );
  }
}