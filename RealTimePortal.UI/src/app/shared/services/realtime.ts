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

  private readonly executionGroups = new Set<string>();
  private dashboardGroupRequested = false;

  private readonly progressUpdateCallbacks =
    new Set<RealtimeCallback>();

  private readonly progressCompletedCallbacks =
    new Set<RealtimeCallback>();

  private readonly dashboardStatusChangedCallbacks =
    new Set<RealtimeCallback>();

  constructor(private auth: Auth) {}

  async start(): Promise<void> {
    if (this.isConnected()) {
      return;
    }

    if (this.startPromise) {
      return this.startPromise;
    }

    this.startPromise = this.connect();

    try {
      await this.startPromise;
    } finally {
      this.startPromise = null;
    }
  }

  private async connect(): Promise<void> {
    if (this.hubConnection) {
      if (this.hubConnection.state !== HubConnectionState.Disconnected) {
        return;
      }

      this.hubConnection = null;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(environment.signalRUrl, {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    this.hubConnection = connection;
    this.registerHubEvents(connection);

    connection.onreconnecting(error => {
      console.warn('SignalR reconnecting...', error);
    });

    connection.onreconnected(async connectionId => {
      console.log('SignalR reconnected:', connectionId);
      await this.rejoinGroups();
    });

    connection.onclose(error => {
      console.warn('SignalR connection closed.', error);
    });

    await connection.start();

    console.log('SignalR connected.');

    await this.rejoinGroups();
  }

  private registerHubEvents(connection: HubConnection): void {
    connection.on('ReceiveProgressUpdate', data => {
      console.log('ReceiveProgressUpdate:', data);

      this.progressUpdateCallbacks.forEach(callback => {
        callback(data);
      });
    });

    connection.on('ReceiveProgressCompleted', data => {
      console.log('ReceiveProgressCompleted:', data);

      this.progressCompletedCallbacks.forEach(callback => {
        callback(data);
      });
    });

    connection.on('dashboardProcessStatusChanged', data => {
      this.dashboardStatusChangedCallbacks.forEach(callback => {
        callback(data);
      });
    });
  }

  async joinDashboard(): Promise<void> {
    this.dashboardGroupRequested = true;

    if (!this.isConnected()) {
      return;
    }

    await this.hubConnection!.invoke('JoinDashboard');
  }

  async leaveDashboard(): Promise<void> {
    this.dashboardGroupRequested = false;

    if (!this.isConnected()) {
      return;
    }

    await this.hubConnection!.invoke('LeaveDashboard');
  }

  async joinExecutionGroup(executionId: string): Promise<void> {
    const normalizedExecutionId = executionId?.trim();

    if (!normalizedExecutionId) {
      return;
    }

    this.executionGroups.add(normalizedExecutionId);

    if (!this.isConnected()) {
      return;
    }

    await this.hubConnection!.invoke(
      'JoinExecutionGroup',
      normalizedExecutionId
    );
  }

  async leaveExecutionGroup(executionId: string): Promise<void> {
    const normalizedExecutionId = executionId?.trim();

    if (!normalizedExecutionId) {
      return;
    }

    this.executionGroups.delete(normalizedExecutionId);

    if (!this.isConnected()) {
      return;
    }

    await this.hubConnection!.invoke(
      'LeaveExecutionGroup',
      normalizedExecutionId
    );
  }

  private async rejoinGroups(): Promise<void> {
    if (!this.isConnected()) {
      return;
    }

    if (this.dashboardGroupRequested) {
      await this.hubConnection!.invoke('JoinDashboard');
    }

    for (const executionId of this.executionGroups) {
      await this.hubConnection!.invoke(
        'JoinExecutionGroup',
        executionId
      );
    }
  }

  private isConnected(): boolean {
    return this.hubConnection?.state === HubConnectionState.Connected;
  }

  onProgressUpdate(callback: RealtimeCallback): void {
    this.progressUpdateCallbacks.add(callback);
  }

  removeProgressUpdate(callback: RealtimeCallback): void {
    this.progressUpdateCallbacks.delete(callback);
  }

  onProgressCompleted(callback: RealtimeCallback): void {
    this.progressCompletedCallbacks.add(callback);
  }

  removeProgressCompleted(callback: RealtimeCallback): void {
    this.progressCompletedCallbacks.delete(callback);
  }

  onDashboardProcessStatusChanged(callback: RealtimeCallback): void {
    this.dashboardStatusChangedCallbacks.add(callback);
  }

  removeDashboardProcessStatusChanged(callback: RealtimeCallback): void {
    this.dashboardStatusChangedCallbacks.delete(callback);
  }

  async stop(): Promise<void> {
    this.executionGroups.clear();
    this.dashboardGroupRequested = false;

    if (!this.hubConnection) {
      return;
    }

    const connection = this.hubConnection;
    this.hubConnection = null;

    await connection.stop();
  }
}
