export type PpeCategory =
  | 'SafetyHelmet'
  | 'SafetyBoots'
  | 'SafetyGlasses'
  | 'EarPlugs'
  | 'EarMuffs'
  | 'Respirator'
  | 'DustMask'
  | 'FaceShield'
  | 'ReflectiveVest'
  | 'Overalls'
  | 'LeatherGloves'
  | 'RubberGloves'
  | 'FallArrestHarness'
  | 'WeldingHelmet'
  | 'FireResistantClothing'
  | 'SafetyGoggles'
  | 'Other';

export type PpeRequestPriority = 'Low' | 'Medium' | 'High' | 'Urgent';

export type PpeRequestStatus =
  | 'Requested'
  | 'PendingApproval'
  | 'Approved'
  | 'Rejected'
  | 'Preparing'
  | 'Dispatched'
  | 'Collected'
  | 'Completed'
  | 'Archived'
  | 'Cancelled';

export type PpeCatalogueItem = {
  id: string;
  itemName: string;
  category: PpeCategory;
  quantityInStock: number;
  minimumStockLevel: number;
  description?: string;
  isActive: boolean;
  isLowStock: boolean;
};

export type PpeRequestListItem = {
  id: string;
  requestNumber: string;
  employeeName: string;
  department: string;
  ppeItemName: string;
  quantity: number;
  priority: PpeRequestPriority;
  status: PpeRequestStatus;
  requestedDate: string;
  requiredByDate: string;
  ageDays: number;
  isOverdue: boolean;
};

export type PpeStatusHistory = {
  id: string;
  oldStatus?: PpeRequestStatus;
  newStatus: PpeRequestStatus;
  action: string;
  actionByUserName: string;
  actionDate: string;
  comments?: string;
};

export type PpeRequestDetail = PpeRequestListItem & {
  employeeUserId: string;
  workDepartmentId?: string;
  location: string;
  section: string;
  ppeCatalogueItemId: string;
  reason: string;
  comments?: string;
  dispatchDate?: string;
  collectedByEmployee?: string;
  collectedDate?: string;
  completedDate?: string;
  dispatchNotes?: string;
  statusHistory: PpeStatusHistory[];
};

export type PpeDashboardKpi = {
  openRequests: number;
  pendingApproval: number;
  preparing: number;
  readyForCollection: number;
  collectedToday: number;
  overdueRequests: number;
  lowStockItems: number;
};

export type PpeReportRow = { label: string; count: number; meta?: string };

export type EmployeePpeHistory = {
  requestId: string;
  requestNumber: string;
  ppeItemName: string;
  requestedDate: string;
  status: PpeRequestStatus;
  dispatchDate?: string;
  collectedDate?: string;
  timeline: PpeStatusHistory[];
};
