export interface EmployeeListItem {
  id: string;
  employeeNumber: string;
  name: string;
  surname: string;
  department: string;
  occupation: string;
  assessmentsCompleted: number;
  nearMissesSubmitted: number;
  status: string;
  isActive: boolean;
}

export interface EmployeeStats {
  totalEmployees: number;
  employeesOnline: number;
  assessmentsCompletedToday: number;
  nearMissesThisMonth: number;
  averageAssessmentsPerEmployee: number;
}

export interface EmployeeAssessment {
  id: string;
  title: string;
  status: string;
  createdDate: string;
  completedDate?: string;
}

export interface EmployeeNearMiss {
  id: string;
  date: string;
  category: string;
  location: string;
  status: string;
}

export interface EmployeeCorrectiveAction {
  id: string;
  description: string;
  assignedDate: string;
  dueDate: string;
  status: string;
}

export interface EmployeeActivity {
  occurredAt: string;
  activityType: string;
  description: string;
}

export interface EmployeeDetail {
  id: string;
  email: string;
  employeeNumber: string;
  name: string;
  surname: string;
  department: string;
  occupation: string;
  companyNumber: string;
  dateJoined: string;
  status: string;
  isActive: boolean;
  assessments: EmployeeAssessment[];
  nearMisses: EmployeeNearMiss[];
  correctiveActions: EmployeeCorrectiveAction[];
  activityTimeline: EmployeeActivity[];
}

export interface EmployeeSearchParams {
  search?: string;
  department?: string;
  occupation?: string;
  page?: number;
  pageSize?: number;
}
