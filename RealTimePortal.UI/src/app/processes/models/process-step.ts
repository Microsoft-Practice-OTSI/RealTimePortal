export interface ProcessStep {
  id: number;
  stepNumber: number;
  stepName: string;
  status: string;
  progressPercentage: number;
  retryCount: number;

  message: string | null;

  inputData: string | null;
  outputData: string | null;

  startedAt: string | null;
  completedAt: string | null;

  errorMessage: string | null;
}