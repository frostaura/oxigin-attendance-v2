export enum TimeEntryStatus {
  Active = 1,
  Completed = 2,
  Cancelled = 3
}

export interface TimeEntry {
  id: number;
  userId: string;
  clockInTime: string;
  clockOutTime?: string;
  totalHours?: string;
  breakTime?: string;
  overtimeHours?: string;
  notes?: string;
  location?: string;
  status: TimeEntryStatus;
  createdAt: string;
  updatedAt: string;
  user?: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    employeeId: string;
    department: string;
    jobTitle: string;
  };
}

export interface ClockInRequest {
  notes?: string;
  location?: string;
}

export interface ClockOutRequest {
  timeEntryId: number;
  notes?: string;
  breakTime?: string;
}

export interface TimeReport {
  userId: string;
  user?: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    employeeId: string;
    department: string;
    jobTitle: string;
  };
  startDate: string;
  endDate: string;
  totalWorkedHours: string;
  totalOvertimeHours: string;
  totalDaysWorked: number;
  timeEntries: TimeEntry[];
}

export interface TimeEntryFilters {
  startDate?: string;
  endDate?: string;
}