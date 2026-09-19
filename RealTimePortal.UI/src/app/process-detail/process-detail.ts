import {
  Component,
  OnDestroy,
  OnInit,
  signal
} from '@angular/core';

import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { firstValueFrom } from 'rxjs';

import { ProcessDetailApi } from './process-detail-api';
import { Process, ProcessStep } from '../processes/models/process';
import { Realtime } from '../shared/services/realtime';

@Component({
  selector: 'app-process-detail',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatTooltipModule
  ],
  templateUrl: './process-detail.html'})
export class ProcessDetail implements OnInit, OnDestroy {

  // ============================================================
  // SIGNALS USED BY process-detail.html
  // ============================================================

  process = signal<Process | null>(null);

  isLoading = signal(false);

  errorMessage = signal('');

  isStarting = signal(false);

  isCancelling = signal(false);

  overallProgress = signal(0);

  signalRConnected = signal(false);

  // ============================================================
  // PRIVATE FIELDS
  // ============================================================

  private processId = 0;

  private executionId = '';

  private destroyed = false;

  private progressRunning = false;

  // ============================================================
  // CONSTRUCTOR
  // ============================================================

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private processDetailApi: ProcessDetailApi,
    private realtime: Realtime
  ) {}

  // ============================================================
  // INIT
  // ============================================================

  async ngOnInit(): Promise<void> {

    this.processId = Number(
      this.route.snapshot.paramMap.get('id')
    );

    if (!this.processId) {

      this.errorMessage.set(
        'Invalid process ID.'
      );

      return;
    }

    this.registerRealtimeEvents();

    await this.loadProcessAndConnect();
  }

  // ============================================================
  // DESTROY
  // ============================================================

  ngOnDestroy(): void {

    this.destroyed = true;

    this.removeRealtimeEvents();

    if (this.executionId) {

      void this.realtime.leaveExecutionGroup(
        this.executionId
      );
    }
  }

  // ============================================================
  // LOAD PROCESS
  // ============================================================

  private async loadProcessAndConnect(): Promise<void> {

    this.isLoading.set(true);

    this.errorMessage.set('');

    this.processDetailApi
      .getById(this.processId)
      .subscribe({

        next: async process => {

          if (this.destroyed) {
            return;
          }

          this.setProcess(process);

          this.isLoading.set(false);

          try {

            await this.realtime.start();

            if (
              this.destroyed ||
              !this.executionId
            ) {
              return;
            }

            await this.realtime.joinExecutionGroup(
              this.executionId
            );

            if (!this.destroyed) {

              this.signalRConnected.set(true);
            }
            if (
    this.getStatusText(process.status) === 'Running' &&
    process.steps?.length > 0
  ) {
    void this.runUiProgress();
  }

          } catch (error) {

            console.error(
              'SignalR connection failed:',
              error
            );

            if (!this.destroyed) {

              this.signalRConnected.set(false);
            }
          }
        },

        error: error => {

          console.error(
            'Unable to load process:',
            error
          );

          if (this.destroyed) {
            return;
          }

          this.isLoading.set(false);

          this.errorMessage.set(
            'Unable to load process. Please try again.'
          );
        }
      });
  }

  loadProcess(): void {

    this.isLoading.set(true);

    this.errorMessage.set('');

    this.processDetailApi
      .getById(this.processId)
      .subscribe({

        next: process => {

          if (this.destroyed) {
            return;
          }

          this.setProcess(process);

          this.isLoading.set(false);
        },

        error: error => {

          console.error(
            'Unable to load process:',
            error
          );

          if (this.destroyed) {
            return;
          }

          this.isLoading.set(false);

          this.errorMessage.set(
            'Unable to load process. Please try again.'
          );
        }
      });
  }

  private setProcess(process: Process): void {

    this.process.set(process);

    this.executionId =
      process.executionId?.trim() ?? '';

    this.calculateOverallProgress();
  }

  // ============================================================
  // START PROCESS
  // ============================================================

  startProcess(): void {

    const currentProcess =
      this.process();

    if (
      !currentProcess ||
      this.isStarting() ||
      this.progressRunning
    ) {
      return;
    }

    this.isStarting.set(true);

    this.errorMessage.set('');

    this.processDetailApi
      .start(currentProcess.id)
      .subscribe({

        next: async () => {

          if (this.destroyed) {
            return;
          }

          this.isStarting.set(false);

          /*
           * Update the local status immediately.
           */
          currentProcess.status = 2;

          this.process.set({
            ...currentProcess
          });

          this.calculateOverallProgress();

          /*
           * Angular now becomes the progress producer
           * for this POC.
           */
          await this.runUiProgress();
        },

        error: error => {

          console.error(
            'Start process error:',
            error
          );

          this.isStarting.set(false);

          this.errorMessage.set(
            'Unable to start the process.'
          );
        }
      });
  }

  // ============================================================
  // UI SENDS PROGRESS TO REALTIMEPORTAL API
  // ============================================================

  private async runUiProgress(): Promise<void> {

    if (
      this.progressRunning ||
      this.destroyed
    ) {
      return;
    }

    const currentProcess =
      this.process();

    if (!currentProcess) {
      return;
    }

    this.progressRunning = true;

    try {

      const steps =
        [...currentProcess.steps]
          .sort(
            (a, b) =>
              a.stepNumber - b.stepNumber
          );

      /*
       * Process each step sequentially.
       */
      for (const step of steps) {

        if (
          this.destroyed ||
          this.isProcessCancelled()
        ) {
          break;
        }

        await this.runStepProgress(step);
      }

      /*
       * Send process-level completion.
       */
      if (
        !this.destroyed &&
        !this.isProcessCancelled()
      ) {

        const latestProcess =
          this.process();

        if (
          latestProcess &&
          latestProcess.steps.length > 0 &&
          latestProcess.steps.every(
            step =>
              this.getStepStatusText(
                step.status
              ) === 'Completed'
          )
        ) {

          await this.sendProgress(
            this.executionId,
            'Process',
            'Completed',
            100,
            'Process completed successfully.'
          );
        }
      }

    } catch (error) {

      console.error(
        'UI progress error:',
        error
      );

      if (!this.destroyed) {

        this.errorMessage.set(
          'Unable to update process progress.'
        );
      }

    } finally {

      this.progressRunning = false;
    }
  }

  private async runStepProgress(
    step: ProcessStep
  ): Promise<void> {

    if (!this.executionId) {
      return;
    }

    /*
     * Step starts.
     */
    await this.sendProgress(
      this.executionId,
      step.stepName,
      'InProgress',
      0,
      `${step.stepName} started.`
    );

    await this.delay(500);

    /*
     * Percentages are controlled by Angular UI.
     */
    const percentages = [
      10,
      25,
      50,
      75,
      100
    ];

    for (const percentage of percentages) {

      if (
        this.destroyed ||
        this.isProcessCancelled()
      ) {
        return;
      }

      const status =
        percentage === 100
          ? 'Completed'
          : 'InProgress';

      const message =
        percentage === 100
          ? `${step.stepName} completed.`
          : `${step.stepName} is ${percentage}% complete.`;

      console.log(
        'Sending progress from UI:',
        {
          executionId: this.executionId,
          step: step.stepName,
          status,
          percentage,
          message
        }
      );

      await this.sendProgress(
        this.executionId,
        step.stepName,
        status,
        percentage,
        message
      );

      if (percentage !== 100) {

        await this.delay(1000);
      }
    }
  }

  private async sendProgress(
    executionId: string,
    step: string,
    status: string,
    percentage: number,
    message: string
  ): Promise<void> {

    if (
      this.destroyed ||
      !executionId
    ) {
      return;
    }

    await firstValueFrom(
      this.processDetailApi.updateProgress(
        executionId,
        step,
        status,
        percentage,
        message
      )
    );
  }

  // ============================================================
  // CANCEL
  // ============================================================

  cancelProcess(): void {

    const currentProcess =
      this.process();

    if (
      !currentProcess ||
      this.isCancelling()
    ) {
      return;
    }

    this.isCancelling.set(true);

    this.errorMessage.set('');

    this.processDetailApi
      .cancel(currentProcess.id)
      .subscribe({

        next: () => {

          if (this.destroyed) {
            return;
          }

          this.isCancelling.set(false);

          currentProcess.status = 5;

          this.process.set({
            ...currentProcess
          });

          this.loadProcess();
        },

        error: error => {

          console.error(
            'Cancel process error:',
            error
          );

          this.isCancelling.set(false);

          this.errorMessage.set(
            'Unable to cancel the process.'
          );
        }
      });
  }

  // ============================================================
  // SIGNALR
  // ============================================================

  private registerRealtimeEvents(): void {

    this.realtime.onProgressUpdate(
      this.onProgressUpdate
    );

    this.realtime.onProgressCompleted(
      this.onProgressCompleted
    );
  }

  private removeRealtimeEvents(): void {

    this.realtime.removeProgressUpdate(
      this.onProgressUpdate
    );

    this.realtime.removeProgressCompleted(
      this.onProgressCompleted
    );
  }

  // ============================================================
  // SIGNALR PROGRESS UPDATE
  // ============================================================

  private readonly onProgressUpdate =
    (data: any): void => {

      console.log(
        'ReceiveProgressUpdate:',
        data
      );

      if (
        !this.isCurrentExecution(
          data?.executionId
        )
      ) {
        return;
      }

      const currentProcess =
        this.process();

      if (!currentProcess) {
        return;
      }

      const stepName =
        String(
          data?.step ?? ''
        ).trim();

      const status =
        String(
          data?.status ?? ''
        ).trim();

      const percentage =
        this.toPercentage(
          data?.percentage
        );

      const message =
        data?.message ?? null;

      /*
       * Overall process update.
       */
      if (
        stepName.toLowerCase() ===
        'process'
      ) {

        this.applyProcessStatus(
          status,
          message
        );

        return;
      }

      /*
       * Find the step.
       */
      const step =
        currentProcess.steps.find(
          item =>
            item.stepName
              .trim()
              .toLowerCase() ===
            stepName.toLowerCase()
        );

      if (!step) {

        console.warn(
          'Progress step not found:',
          stepName
        );

        return;
      }

      /*
       * Update step from SignalR.
       */
      step.progressPercentage =
        percentage;

      if (message !== null) {

        step.message =
          message;
      }

      const normalizedStatus =
        this.normalizeStatus(
          status
        );

      if (
        normalizedStatus ===
        'completed'
      ) {

        step.status =
          3;

        step.progressPercentage =
          100;

        step.completedAt =
          step.completedAt ??
          new Date().toISOString();

      } else if (
        normalizedStatus ===
        'failed'
      ) {

        step.status =
          4;

        step.errorMessage =
          message ||
          'Step failed.';

      } else if (
        normalizedStatus ===
        'cancelled'
      ) {

        step.status =
          5;

      } else {

        step.status =
          2;
      }

      /*
       * Replace the object so Angular signals
       * notify the template.
       */
      this.process.set({
        ...currentProcess,
        steps: [
          ...currentProcess.steps
        ]
      });

      this.calculateOverallProgress();
    };

  // ============================================================
  // SIGNALR COMPLETION
  // ============================================================

  private readonly onProgressCompleted =
    (data: any): void => {

      console.log(
        'ReceiveProgressCompleted:',
        data
      );

      if (
        !this.isCurrentExecution(
          data?.executionId
        )
      ) {
        return;
      }

      const currentProcess =
        this.process();

      if (!currentProcess) {
        return;
      }

      currentProcess.status =
        3;

      currentProcess.completedAt =
        currentProcess.completedAt ??
        new Date().toISOString();

      currentProcess.steps.forEach(
        step => {

          const status =
            this.getStepStatusText(
              step.status
            );

          if (
            status !== 'Failed' &&
            status !== 'Skipped'
          ) {

            step.status =
              3;

            step.progressPercentage =
              100;

            step.completedAt =
              step.completedAt ??
              currentProcess.completedAt;
          }
        }
      );

      this.process.set({
        ...currentProcess,
        steps: [
          ...currentProcess.steps
        ]
      });

      this.overallProgress.set(100);
    };

  // ============================================================
  // PROCESS STATUS
  // ============================================================

  private applyProcessStatus(
    status: string,
    message: string | null
  ): void {

    const currentProcess =
      this.process();

    if (!currentProcess) {
      return;
    }

    const normalizedStatus =
      this.normalizeStatus(
        status
      );

    switch (normalizedStatus) {

      case 'completed':

        currentProcess.status =
          3;

        currentProcess.completedAt =
          currentProcess.completedAt ??
          new Date().toISOString();

        break;

      case 'failed':

        currentProcess.status =
          4;

        currentProcess.errorMessage =
          message ||
          'Process failed.';

        break;

      case 'cancelled':

        currentProcess.status =
          5;

        break;

      default:

        currentProcess.status =
          2;

        break;
    }

    this.process.set({
      ...currentProcess
    });

    this.calculateOverallProgress();
  }

  // ============================================================
  // OVERALL PROGRESS
  // ============================================================

  calculateOverallProgress(): void {

    const currentProcess =
      this.process();

    if (
      !currentProcess ||
      !currentProcess.steps?.length
    ) {

      this.overallProgress.set(0);

      return;
    }

    const total =
      currentProcess.steps.reduce(
        (sum, step) =>
          sum +
          this.toPercentage(
            step.progressPercentage
          ),
        0
      );

    let progress =
      Math.round(
        total /
        currentProcess.steps.length
      );

    if (
      this.getStatusText(
        currentProcess.status
      ) === 'Completed'
    ) {

      progress = 100;
    }

    this.overallProgress.set(
      progress
    );
  }

  // ============================================================
  // TEMPLATE GETTERS
  // ============================================================

  get completedStepCount(): number {

    const currentProcess =
      this.process();

    if (!currentProcess) {
      return 0;
    }

    return currentProcess.steps.filter(
      step => {

        const status =
          this.getStepStatusText(
            step.status
          );

        return (
          status === 'Completed' ||
          status === 'Skipped'
        );
      }
    ).length;
  }

  getProgressMessage(): string {

    const currentProcess =
      this.process();

    if (!currentProcess) {
      return '';
    }

    const status =
      this.getStatusText(
        currentProcess.status
      );

    if (status === 'Completed') {

      return 'Process completed successfully.';
    }

    if (status === 'Failed') {

      return (
        currentProcess.errorMessage ||
        'Process failed.'
      );
    }

    if (status === 'Cancelled') {

      return 'Process was cancelled.';
    }

    const activeStep =
      currentProcess.steps.find(
        step =>
          this.getStepStatusText(
            step.status
          ) === 'In Progress'
      );

    if (activeStep) {

      return (
        activeStep.message ||
        activeStep.stepName
      );
    }

    if (status === 'Running') {

      return 'Process is running.';
    }

    return 'Ready to start.';
  }

  // ============================================================
  // STATUS HELPERS
  // ============================================================

  getStatusText(
    status: string | number
  ): string {

    if (typeof status === 'string') {

      const value =
        status
          .trim()
          .toLowerCase();

      switch (value) {

        case 'pending':
          return 'Pending';

        case 'running':
        case 'inprogress':
        case 'in progress':
          return 'Running';

        case 'completed':
        case 'complete':
          return 'Completed';

        case 'failed':
        case 'failure':
          return 'Failed';

        case 'cancelled':
        case 'canceled':
          return 'Cancelled';

        default:
          return this.capitalize(
            status
          );
      }
    }

    switch (status) {

      case 1:
        return 'Pending';

      case 2:
        return 'Running';

      case 3:
        return 'Completed';

      case 4:
        return 'Failed';

      case 5:
        return 'Cancelled';

      default:
        return 'Unknown';
    }
  }

  getStatusClass(
    status: string | number
  ): string {

    return this.getStatusText(status)
      .toLowerCase()
      .replace(/\s+/g, '-');
  }

  getStepStatusText(
    status: string | number
  ): string {

    if (typeof status === 'string') {

      const value =
        status
          .trim()
          .toLowerCase();

      switch (value) {

        case 'pending':
          return 'Pending';

        case 'running':
        case 'inprogress':
        case 'in progress':
          return 'In Progress';

        case 'completed':
        case 'complete':
          return 'Completed';

        case 'failed':
        case 'failure':
          return 'Failed';

        case 'skipped':
          return 'Skipped';

        default:
          return this.capitalize(
            status
          );
      }
    }

    switch (status) {

      case 1:
        return 'Pending';

      case 2:
        return 'In Progress';

      case 3:
        return 'Completed';

      case 4:
        return 'Failed';

      case 5:
        return 'Skipped';

      default:
        return 'Unknown';
    }
  }

  getStepStatusClass(
    status: string | number
  ): string {

    return this.getStepStatusText(status)
      .toLowerCase()
      .replace(/\s+/g, '-');
  }

  getStepProgress(
    step: ProcessStep
  ): number {

    return this.toPercentage(
      step.progressPercentage
    );
  }

  getDisplayName(
    processType: string
  ): string {

    return processType
      .replace(
        /([a-z])([A-Z])/g,
        '$1 $2'
      )
      .replace(
        /[-_]/g,
        ' '
      )
      .replace(
        /\w\S*/g,
        word =>
          word.charAt(0).toUpperCase() +
          word.slice(1).toLowerCase()
      );
  }

  // ============================================================
  // HELPERS
  // ============================================================

  private isCurrentExecution(
    executionId: any
  ): boolean {

    if (
      !executionId ||
      !this.executionId
    ) {
      return false;
    }

    return (
      String(executionId)
        .trim()
        .toLowerCase() ===
      this.executionId
        .trim()
        .toLowerCase()
    );
  }

  private isProcessCancelled(): boolean {

    const currentProcess =
      this.process();

    if (!currentProcess) {
      return true;
    }

    return (
      this.getStatusText(
        currentProcess.status
      ) === 'Cancelled'
    );
  }

  private normalizeStatus(
    status: string
  ): string {

    const value =
      status
        .trim()
        .toLowerCase();

    switch (value) {

      case 'completed':
      case 'complete':
      case 'success':
        return 'completed';

      case 'failed':
      case 'failure':
      case 'error':
        return 'failed';

      case 'cancelled':
      case 'canceled':
        return 'cancelled';

      default:
        return 'inprogress';
    }
  }

  private toPercentage(
    value: any
  ): number {

    const percentage =
      Number(value ?? 0);

    if (
      !Number.isFinite(
        percentage
      )
    ) {
      return 0;
    }

    return Math.max(
      0,
      Math.min(
        100,
        percentage
      )
    );
  }

  private capitalize(
    value: string
  ): string {

    if (!value) {
      return value;
    }

    return (
      value.charAt(0).toUpperCase() +
      value.slice(1)
    );
  }

  private delay(
    milliseconds: number
  ): Promise<void> {

    return new Promise(
      resolve =>
        setTimeout(
          resolve,
          milliseconds
        )
    );
  }

  // ============================================================
  // NAVIGATION
  // ============================================================

  goBack(): void {

    this.router.navigate([
      '/processes'
    ]);
  }
}