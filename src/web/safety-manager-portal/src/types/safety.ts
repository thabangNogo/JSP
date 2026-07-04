export interface WorkLookupItem {
  id: string;
  name: string;
  sortOrder: number;
}

export interface WorkLookups {
  departments: WorkLookupItem[];
  locations: WorkLookupItem[];
  sections: WorkLookupItem[];
}

export interface JobSafetyAssessment {
  id: string;
  title: string;
  department: string;
  location: string;
  section: string;
  status: string;
  signOffName?: string;
  signOffSurname?: string;
  lastSavedAt?: string;
  validFrom?: string;
}

export interface NearMiss {
  id: string;
  department: string;
  location: string;
  section: string;
  category: string;
  description: string;
  occurredAt: string;
  status: string;
}

export interface StopUnsafeWorkReport {
  id: string;
  department: string;
  location: string;
  section: string;
  category: string;
  description: string;
  immediateRisk: string;
  status: string;
  createdDate: string;
}

export interface CreateEmployeePayload {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  employeeNumber: string;
  workDepartmentId: string;
  companyNumber: string;
  occupation: string;
  roles: string[];
}
