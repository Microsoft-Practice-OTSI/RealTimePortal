namespace RealTimePortal.Domain;

public enum ProcessStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum ProcessStepStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Skipped = 5
}

public enum DashboardPeriod
{
    AllTime = 1,
    Today = 2,
    Last7Days = 3,
    Last30Days = 4
}