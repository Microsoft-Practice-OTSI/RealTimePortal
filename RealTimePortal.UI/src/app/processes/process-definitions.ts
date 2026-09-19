export interface ProcessDefinition {
  processType: string;

  displayName: string;

  description: string;

  icon: string;

  colorClass: string;

  steps: string[];
}


export const PROCESS_DEFINITIONS:
  ProcessDefinition[] = [

  {
    processType: 'WorkOrder',

    displayName: 'Work Order',

    description:
      'Create and process a work order.',

    icon: 'assignment',

    colorClass: 'blue',

    steps: [
      'Pre Validation',
      'Create Work Order',
      'Create Documents',
      'Create Confirmations',
      'Generate Invoice'
    ]
  },


  {
    processType: 'FileProcessing',

    displayName: 'File Processing',

    description:
      'Validate, upload and process a file.',

    icon: 'description',

    colorClass: 'purple',

    steps: [
      'Validate File',
      'Upload File',
      'Process File',
      'Save Result'
    ]
  },


  {
    processType: 'ReportGeneration',

    displayName: 'Report Generation',

    description:
      'Generate and save a business report.',

    icon: 'assessment',

    colorClass: 'orange',

    steps: [
      'Validate Request',
      'Collect Data',
      'Generate Report',
      'Save Report'
    ]
  },


  {
    processType: 'EmployeeOnboarding',

    displayName: 'Employee Onboarding',

    description:
      'Create and onboard a new employee.',

    icon: 'person_add',

    colorClass: 'green',

    steps: [
      'Validate Employee',
      'Create Employee',
      'Create Documents',
      'Create Account',
      'Send Notification'
    ]
  }

];