import { Injectable } from '@angular/core';

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState
} from '@microsoft/signalr';

import { environment } from '../../../environments/environment';
import { Auth } from '../../auth/auth';

type RealtimeCallback = (data: any) => void;

@Injectable({
  providedIn: 'root'
})
export class Realtime {

  private hubConnection: HubConnection | null = null;

  private startPromise: Promise<void> | null = null;


  // =========================================================
  // SIGNALR GROUPS
  // =========================================================

  private readonly executionGroups =
    new Set<string>();

  private readonly conversationGroups =
    new Set<number>();

  private dashboardGroupRequested =
    false;


  // =========================================================
  // PROGRESS CALLBACKS
  // =========================================================

  private readonly progressUpdateCallbacks =
    new Set<RealtimeCallback>();

  private readonly progressCompletedCallbacks =
    new Set<RealtimeCallback>();


  // =========================================================
  // DASHBOARD CALLBACKS
  // =========================================================

  private readonly dashboardStatusChangedCallbacks =
    new Set<RealtimeCallback>();


  // =========================================================
  // CHAT CALLBACKS
  // =========================================================

  private readonly chatMessageCallbacks =
    new Set<RealtimeCallback>();


    private readonly chatUnreadMessageCallbacks =
  new Set<RealtimeCallback>();

  constructor(
    private readonly auth: Auth
  ) {}


  // =========================================================
  // CONNECTION
  // =========================================================

  async start(): Promise<void> {

    if (this.isConnected()) {
      return;
    }

    if (this.startPromise) {
      return this.startPromise;
    }

    this.startPromise =
      this.connect();

    try {

      await this.startPromise;

    } finally {

      this.startPromise = null;
    }
  }


  private async connect(): Promise<void> {

    if (this.hubConnection) {

      if (
        this.hubConnection.state !==
        HubConnectionState.Disconnected
      ) {
        return;
      }

      this.hubConnection = null;
    }


    const connection =
      new HubConnectionBuilder()

        .withUrl(
          environment.signalRUrl,
          {
            accessTokenFactory: () =>
              this.auth.getToken() ?? ''
          }
        )

        .withAutomaticReconnect(
          [
            0,
            2000,
            5000,
            10000
          ]
        )

        .build();


    this.hubConnection =
      connection;


    this.registerHubEvents(
      connection
    );


    // =======================================================
    // RECONNECTING
    // =======================================================

    connection.onreconnecting(
      error => {

        console.warn(
          'SignalR reconnecting...',
          error
        );

      }
    );


    // =======================================================
    // RECONNECTED
    // =======================================================

    connection.onreconnected(
      async connectionId => {

        console.log(
          'SignalR reconnected:',
          connectionId
        );

        await this.rejoinGroups();

      }
    );


    // =======================================================
    // CLOSED
    // =======================================================

    connection.onclose(
      error => {

        console.warn(
          'SignalR connection closed.',
          error
        );

      }
    );


    // =======================================================
    // START CONNECTION
    // =======================================================

    await connection.start();


    console.log(
      'SignalR connected.'
    );


    // Rejoin requested groups after
    // initial connection as well.
    await this.rejoinGroups();
  }


  // =========================================================
  // SIGNALR EVENT REGISTRATION
  // =========================================================

  private registerHubEvents(
    connection: HubConnection
  ): void {


    // =======================================================
    // PROGRESS UPDATE
    // =======================================================

    connection.on(
      'ReceiveProgressUpdate',
      data => {

        console.log(
          'ReceiveProgressUpdate:',
          data
        );

        this.progressUpdateCallbacks
          .forEach(callback => {

            callback(data);

          });
      }
    );


    // =======================================================
    // PROGRESS COMPLETED
    // =======================================================

    connection.on(
      'ReceiveProgressCompleted',
      data => {

        console.log(
          'ReceiveProgressCompleted:',
          data
        );

        this.progressCompletedCallbacks
          .forEach(callback => {

            callback(data);

          });
      }
    );


    // =======================================================
    // DASHBOARD STATUS CHANGED
    // =======================================================

    connection.on(
      'dashboardProcessStatusChanged',
      data => {

        console.log(
          'dashboardProcessStatusChanged:',
          data
        );

        this.dashboardStatusChangedCallbacks
          .forEach(callback => {

            callback(data);

          });
      }
    );


    // =======================================================
    // CHAT MESSAGE
    // =======================================================

   connection.on(
      'chatMessageReceived',
      data => {

        console.log(
          'chatMessageReceived:',
          data
        );

        this.chatMessageCallbacks
          .forEach(callback => {
            callback(data);
          });
      }
    );

    connection.on(
      'chatUnreadMessageReceived',
      data => {
        console.log(
          'chatUnreadMessageReceived:',
          data
        );

        this.chatUnreadMessageCallbacks
          .forEach(callback => callback(data));
      }
    );
  }

  



  // =========================================================
  // DASHBOARD GROUP
  // =========================================================

  async joinDashboard(): Promise<void> {

    this.dashboardGroupRequested =
      true;


    if (!this.isConnected()) {
      return;
    }


    await this.hubConnection!
      .invoke(
        'JoinDashboard'
      );
  }


  async leaveDashboard(): Promise<void> {

    this.dashboardGroupRequested =
      false;


    if (!this.isConnected()) {
      return;
    }


    await this.hubConnection!
      .invoke(
        'LeaveDashboard'
      );
  }


  // =========================================================
  // EXECUTION GROUP
  // =========================================================

  async joinExecutionGroup(
    executionId: string
  ): Promise<void> {

    const normalizedExecutionId =
      executionId?.trim();


    if (!normalizedExecutionId) {
      return;
    }


    this.executionGroups.add(
      normalizedExecutionId
    );


    if (!this.isConnected()) {
      return;
    }


    await this.hubConnection!
      .invoke(
        'JoinExecutionGroup',
        normalizedExecutionId
      );
  }


  async leaveExecutionGroup(
    executionId: string
  ): Promise<void> {

    const normalizedExecutionId =
      executionId?.trim();


    if (!normalizedExecutionId) {
      return;
    }


    this.executionGroups.delete(
      normalizedExecutionId
    );


    if (!this.isConnected()) {
      return;
    }


    await this.hubConnection!
      .invoke(
        'LeaveExecutionGroup',
        normalizedExecutionId
      );
  }


  // =========================================================
  // CHAT GROUP
  // =========================================================

  async joinConversation(
    conversationId: number
  ): Promise<void> {

    if (!conversationId) {
      return;
    }


    // Remember the group so it can be
    // rejoined after reconnect.
    this.conversationGroups.add(
      conversationId
    );


    if (!this.isConnected()) {
      return;
    }


    await this.hubConnection!
      .invoke(
        'JoinConversation',
        conversationId
      );


    console.log(
      'Joined chat conversation:',
      conversationId
    );
  }


  async leaveConversation(
    conversationId: number
  ): Promise<void> {

    if (!conversationId) {
      return;
    }


    this.conversationGroups.delete(
      conversationId
    );


    if (!this.isConnected()) {
      return;
    }


    await this.hubConnection!
      .invoke(
        'LeaveConversation',
        conversationId
      );


    console.log(
      'Left chat conversation:',
      conversationId
    );
  }


  // =========================================================
  // REJOIN GROUPS
  // =========================================================

  private async rejoinGroups(): Promise<void> {

    if (!this.isConnected()) {
      return;
    }


    // -------------------------------------------------------
    // Dashboard
    // -------------------------------------------------------

    if (
      this.dashboardGroupRequested
    ) {

      await this.hubConnection!
        .invoke(
          'JoinDashboard'
        );
    }


    // -------------------------------------------------------
    // Process execution groups
    // -------------------------------------------------------

    for (
      const executionId
      of this.executionGroups
    ) {

      await this.hubConnection!
        .invoke(
          'JoinExecutionGroup',
          executionId
        );
    }


    // -------------------------------------------------------
    // Chat conversation groups
    // -------------------------------------------------------

    for (
      const conversationId
      of this.conversationGroups
    ) {

      await this.hubConnection!
        .invoke(
          'JoinConversation',
          conversationId
        );

      console.log(
        'Rejoined chat conversation:',
        conversationId
      );
    }
  }


  // =========================================================
  // CONNECTION STATE
  // =========================================================

  private isConnected(): boolean {

    return (
      this.hubConnection?.state ===
      HubConnectionState.Connected
    );
  }


  // =========================================================
  // PROGRESS CALLBACKS
  // =========================================================

  onProgressUpdate(
    callback: RealtimeCallback
  ): void {

    this.progressUpdateCallbacks.add(
      callback
    );
  }


  removeProgressUpdate(
    callback: RealtimeCallback
  ): void {

    this.progressUpdateCallbacks.delete(
      callback
    );
  }


  onProgressCompleted(
    callback: RealtimeCallback
  ): void {

    this.progressCompletedCallbacks.add(
      callback
    );
  }


  removeProgressCompleted(
    callback: RealtimeCallback
  ): void {

    this.progressCompletedCallbacks.delete(
      callback
    );
  }


  // =========================================================
  // DASHBOARD CALLBACKS
  // =========================================================

  onDashboardProcessStatusChanged(
    callback: RealtimeCallback
  ): void {

    this.dashboardStatusChangedCallbacks.add(
      callback
    );
  }


  removeDashboardProcessStatusChanged(
    callback: RealtimeCallback
  ): void {

    this.dashboardStatusChangedCallbacks.delete(
      callback
    );
  }


  // =========================================================
  // CHAT CALLBACKS
  // =========================================================

  onChatMessage(
    callback: RealtimeCallback
  ): void {

    this.chatMessageCallbacks.add(
      callback
    );
  }


  removeChatMessage(
    callback: RealtimeCallback
  ): void {

    this.chatMessageCallbacks.delete(
      callback
    );
  }


  registerChatMessageHandler(
  callback: RealtimeCallback
): void {
  this.chatMessageCallbacks.add(callback);
}


registerChatUnreadMessageHandler(
  callback: RealtimeCallback
): void {
  this.chatUnreadMessageCallbacks.add(
    callback
  );
}


  // =========================================================
  // STOP
  // =========================================================

  async stop(): Promise<void> {

    // Clear all remembered groups.
    this.executionGroups.clear();

    this.conversationGroups.clear();

    this.dashboardGroupRequested =
      false;


    if (!this.hubConnection) {
      return;
    }


    const connection =
      this.hubConnection;


    this.hubConnection =
      null;


    await connection.stop();
  }
}