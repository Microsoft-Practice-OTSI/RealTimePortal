export interface DashboardProcess {
  id: number;
  clientId: string;
  applicationId: string;
  processType: string;
  referenceNumber: string;
  status: string;
  startedAt: string | null;
  completedAt: string | null;
  errorMessage: string | null;
  createdAt: string;
}