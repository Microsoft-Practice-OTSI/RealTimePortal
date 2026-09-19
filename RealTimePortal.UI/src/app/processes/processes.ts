import {
  Component,
  OnInit,
  signal
} from '@angular/core';

import {
  CommonModule,
  DatePipe
} from '@angular/common';

import {
  Router
} from '@angular/router';

import {
  MatButtonModule
} from '@angular/material/button';

import {
  MatIconModule
} from '@angular/material/icon';

import {
  MatProgressSpinnerModule
} from '@angular/material/progress-spinner';

import {
  FormsModule
} from '@angular/forms';

import {
  ProcessesApi
} from './processes-api';

import {
  Process
} from './models/process';

import {
  ProcessDefinition,
  PROCESS_DEFINITIONS
} from './process-definitions';


@Component({
  selector: 'app-processes',

  standalone: true,

  imports: [
    CommonModule,
    FormsModule,
    DatePipe,

    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],

  templateUrl: './processes.html'})
export class Processes
  implements OnInit {


  isLoading = signal(false);

  isStarting = signal(false);

  errorMessage = signal('');


  processes: Process[] = [];


  processDefinitions:
    ProcessDefinition[] =
      PROCESS_DEFINITIONS;


  searchText = '';

  selectedProcessType = '';

  selectedStatus = '';


  constructor(
    private processesApi: ProcessesApi,

    private router: Router
  ) {}


  ngOnInit(): void {

    this.loadProcesses();
  }


  loadProcesses(): void {

    this.isLoading.set(true);

    this.errorMessage.set('');


    this.processesApi
      .getAll()
      .subscribe({

        next: result => {

          this.processes =
            result ?? [];

          this.isLoading.set(false);
        },


        error: error => {

          console.error(
            'Processes API error:',
            error
          );

          this.isLoading.set(false);

          this.errorMessage.set(
            'Unable to load processes. Please try again.'
          );
        }

      });
  }


  get filteredProcesses(): Process[] {

    const search =
      this.searchText
        .trim()
        .toLowerCase();


    return this.processes.filter(
      process => {

        const matchesSearch =
          !search ||
          process.referenceNumber
            ?.toLowerCase()
            .includes(search) ||
          process.processType
            ?.toLowerCase()
            .includes(search) ||
          String(process.id)
            .includes(search);


        const matchesType =
          !this.selectedProcessType ||
          process.processType ===
            this.selectedProcessType;


        const matchesStatus =
          !this.selectedStatus ||
          this.getStatusText(
            process.status
          ) === this.selectedStatus;


        return (
          matchesSearch &&
          matchesType &&
          matchesStatus
        );
      }
    );
  }


  async startProcess(
    definition: ProcessDefinition
  ): Promise<void> {

    if (this.isStarting()) {
      return;
    }


    this.isStarting.set(true);

    this.errorMessage.set('');


    const referenceNumber =
      `${this.getReferencePrefix(definition.processType)}-${Date.now()}`;


    const request = {

      /*
       * These are generic API fields.
       *
       * The business application would normally
       * supply its own ClientId/ApplicationId.
       */

      clientId: 'DemoClient',

      applicationId: 'RealTimePortalDemo',

      processType:
        definition.processType,

      referenceNumber,

      requestData: null,

      steps:
        definition.steps.map(
          (stepName, index) => ({

            stepNumber: index + 1,

            stepName,

            inputData: null

          })
        )
    };


    this.processesApi
      .create(request)
      .subscribe({

        next: result => {

          this.isStarting.set(false);


          const processId =
            Number(result?.id);


          if (processId) {

            /*
             * Start the generic process.
             */

            this.processesApi
              .start(processId)
              .subscribe({

                next: () => {

                  this.router.navigate([
                    '/processes',
                    processId
                  ]);
                },


                error: error => {

                  console.error(
                    'Start process error:',
                    error
                  );

                  this.errorMessage.set(
                    'Process was created but could not be started.'
                  );

                  this.loadProcesses();
                }

              });

          } else {

            this.loadProcesses();
          }
        },


        error: error => {

          console.error(
            'Create process error:',
            error
          );

          this.isStarting.set(false);

          this.errorMessage.set(
            'Unable to create the process.'
          );
        }

      });
  }


  viewProcess(
    processId: number
  ): void {

    this.router.navigate([
      '/processes',
      processId
    ]);
  }


  refresh(): void {

    this.loadProcesses();
  }


  clearFilters(): void {

    this.searchText = '';

    this.selectedProcessType = '';

    this.selectedStatus = '';
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
          return this.capitalize(status);
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


  getDisplayName(
    processType: string
  ): string {

    const definition =
      this.processDefinitions.find(
        x =>
          x.processType ===
          processType
      );


    if (definition) {
      return definition.displayName;
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


  private getProcessPrefix(
    processType: string
  ): string {

    return processType
      .replace(/([a-z])([A-Z])/g, '$1-$2')
      .toUpperCase();
  }


  private getReferencePrefix(
    processType: string
  ): string {

    switch (processType) {

      case 'WorkOrder':
        return 'WO';

      case 'FileProcessing':
        return 'FILE';

      case 'ReportGeneration':
        return 'RPT';

      case 'EmployeeOnboarding':
        return 'EMP';

      default:
        return 'PROC';
    }
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