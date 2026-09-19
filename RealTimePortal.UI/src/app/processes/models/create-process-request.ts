export interface CreateProcessRequest {
  clientId: string;
  applicationId: string;
  processType: string;
  referenceNumber: string;
  requestData?: string | null;
  steps: CreateProcessStepRequest[];
}

export interface CreateProcessStepRequest {
  stepNumber: number;
  stepName: string;
  inputData?: string | null;
}