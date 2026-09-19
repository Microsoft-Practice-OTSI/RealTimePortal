export interface ProcessTypeSummary {
  processType: string;
  total: number;
  pending: number;
  running: number;
  completed: number;
  failed: number;
  cancelled: number;
}