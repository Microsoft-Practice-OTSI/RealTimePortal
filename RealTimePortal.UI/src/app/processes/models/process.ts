export interface Process {
  id: number;
  executionId: string;

  clientId: string;
  applicationId: string;
  processType: string;
  referenceNumber: string;

  status: string | number;

  startedByUserId: number | null;
  startedAt: string | null;
  completedAt: string | null;

  errorMessage: string | null;
  requestData: string | null;

  createdAt: string;
  createdBy: string | null;
  modifiedAt: string | null;
  modifiedBy: string | null;

  steps: ProcessStep[];
}

export interface ProcessStep {
  id: number;
  processId: number;
  stepNumber: number;
  stepName: string;

  status: string | number;
  progressPercentage: number;
  retryCount: number;

  message: string | null;
  inputData: string | null;
  outputData: string | null;

  startedAt: string | null;
  completedAt: string | null;
  errorMessage: string | null;
}
