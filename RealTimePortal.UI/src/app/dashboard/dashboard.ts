import {
  Component,
  OnDestroy,
  OnInit,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';

import { forkJoin } from 'rxjs';

import {
  MatButtonModule
} from '@angular/material/button';

import {
  MatCardModule
} from '@angular/material/card';

import {
  MatProgressSpinnerModule
} from '@angular/material/progress-spinner';

import {
  MatIconModule
} from '@angular/material/icon';

import {
  ChartConfiguration,
  ChartOptions
} from 'chart.js';

import {
  BaseChartDirective
} from 'ng2-charts';

import { DashboardApi } from './dashboard-api';

import { DashboardSummary } from './models/dashboard-summary';
import { ProcessTypeSummary } from './models/process-type-summary';
import { DashboardProcess } from './models/dashboard-process';
import { DashboardTrend } from './models/dashboard-trend';

import { Realtime } from '../shared/services/realtime';


interface ProcessTypeDashboard {
  summary: ProcessTypeSummary;
  trend: DashboardTrend[];
  chartData: ChartConfiguration<'line'>['data'];
}


@Component({
  selector: 'app-dashboard',

  standalone: true,

  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule,

    BaseChartDirective
  ],

  templateUrl: './dashboard.html'})
export class Dashboard
  implements OnInit, OnDestroy {


  isLoading = signal(false);

  errorMessage = signal('');

  // Backend enum: AllTime=1, Today=2, Last7Days=3, Last30Days=4.
  selectedPeriod = 4;


  summary: DashboardSummary | null = null;

  processTypes: ProcessTypeDashboard[] = [];

  recentProcesses: DashboardProcess[] = [];

  runningProcesses: DashboardProcess[] = [];

  failedProcesses: DashboardProcess[] = [];


  signalRConnected = false;


  private readonly chartColors = [
    '#2196F3',
    '#8B5CF6',
    '#14B8A6',
    '#F59E0B',
    '#EC4899',
    '#10B981',
    '#EF4444',
    '#6366F1'
  ];


  private readonly dashboardStatusChanged =
    (data: any): void => {

      console.log(
        'Dashboard process status changed:',
        data
      );

      /*
       * API remains the source of truth.
       *
       * SignalR tells us that something changed.
       * We then reload the dashboard from the API.
       */

      this.loadDashboard();
    };


  constructor(
    private dashboardApi: DashboardApi,

    private realtime: Realtime
  ) {}


  async ngOnInit(): Promise<void> {

    this.realtime
      .onDashboardProcessStatusChanged(
        this.dashboardStatusChanged
      );


    this.loadDashboard();


    try {

      await this.realtime.start();

      await this.realtime.joinDashboard();

      this.signalRConnected = true;

    } catch (error) {

      console.error(
        'SignalR connection failed:',
        error
      );

      this.signalRConnected = false;
    }
  }


  ngOnDestroy(): void {

    this.realtime
      .removeDashboardProcessStatusChanged(
        this.dashboardStatusChanged
      );


    void this.realtime.leaveDashboard();

    /*
     * Do not stop SignalR here.
     *
     * Realtime is shared by Dashboard,
     * Process Detail and Chat.
     */
  }


  changePeriod(
    period: number
  ): void {

    if (
      this.selectedPeriod === period
    ) {
      return;
    }


    this.selectedPeriod = period;

    this.loadDashboard();
  }


  loadDashboard(): void {

    this.isLoading.set(true);

    this.errorMessage.set('');


    forkJoin({

      summary:
        this.dashboardApi.getSummary(
          this.selectedPeriod
        ),

      processTypes:
        this.dashboardApi.getProcessTypes(
          this.selectedPeriod
        ),

      recentProcesses:
        this.dashboardApi.getRecentProcesses(
          this.selectedPeriod,
          10
        ),

      runningProcesses:
        this.dashboardApi.getRunningProcesses(
          this.selectedPeriod
        ),

      failedProcesses:
        this.dashboardApi.getFailedProcesses(
          this.selectedPeriod,
          10
        ),

      trend:
        this.dashboardApi.getTrend(
          this.selectedPeriod
        )

    }).subscribe({

      next: result => {

        this.summary =
          result.summary;


        this.recentProcesses =
          result.recentProcesses;


        this.runningProcesses =
          result.runningProcesses;


        this.failedProcesses =
          result.failedProcesses;


        this.processTypes =
          result.processTypes.map(
            summary => {

              const processTrend =
                result.trend.filter(
                  item =>
                    item.processType ===
                    summary.processType
                );


              return {
                summary,
                trend: processTrend,
                chartData:
                  this.createChartData(
                    summary.processType,
                    processTrend
                  )
              };

            })
             .sort((a, b) =>
              b.summary.processType.localeCompare(
                a.summary.processType,
                undefined,
                {
                  sensitivity: 'base'
                }
              )
            );
          
        this.isLoading.set(false);
      },


      error: error => {

        console.error(
          'Dashboard API error:',
          error
        );


        this.isLoading.set(false);

        this.errorMessage.set(
          'Unable to load dashboard data. Please try again.'
        );
      }

    });
  }


  private createChartData(
    processType: string,
    trend: DashboardTrend[]
  ): ChartConfiguration<'line'>['data'] {

    const dates = [
      ...new Set(
        trend.map(item => item.date)
      )
    ].sort();


    const color =
      this.getProcessTypeColor(
        processType
      );


    return {

      labels:
        dates.map(date =>
          new Date(date).toLocaleDateString(
            'en-GB',
            {
              day: '2-digit',
              month: 'short'
            }
          )
        ),


      datasets: [

        {
          label: 'Created',

          data: dates.map(date => {

            const item =
              trend.find(
                x =>
                  x.date === date
              );

            return item?.created ?? 0;
          }),

          borderColor: color,

          backgroundColor: color,

          pointBackgroundColor: color,

          pointBorderColor: color,

          borderWidth: 2,

          pointRadius: 3,

          pointHoverRadius: 5,

          tension: .35,

          fill: false
        }

      ]
    };
  }


  private getProcessTypeColor(
    processType: string
  ): string {

    let hash = 0;


    for (
      let i = 0;
      i < processType.length;
      i++
    ) {

      hash =
        processType.charCodeAt(i) +
        ((hash << 5) - hash);

    }


    const index =
      Math.abs(hash) %
      this.chartColors.length;


    return this.chartColors[index];
  }


  getStatusText(
    status: string | number
  ): string {

    if (
      typeof status === 'string'
    ) {

      const value =
        status.toLowerCase();


      switch (value) {

        case 'pending':
          return 'Pending';

        case 'running':
        case 'in progress':
          return 'Running';

        case 'completed':
          return 'Completed';

        case 'failed':
          return 'Failed';

        case 'cancelled':
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

    return this
      .getStatusText(status)
      .toLowerCase();
  }


  getProcessDisplayName(
    processType: string
  ): string {

    if (!processType) {
      return 'Process';
    }


    return processType
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[-_]/g, ' ')
      .replace(
        /\w\S*/g,
        word =>
          word.charAt(0).toUpperCase() +
          word.slice(1).toLowerCase()
      );
  }


  private capitalize(
    value: string
  ): string {

    if (!value) {
      return value;
    }


    return value.charAt(0).toUpperCase() +
      value.slice(1);
  }
}