import { apiClient, unwrap } from './axiosClient';
import type {
  EmployeePpeHistory,
  PpeCatalogueItem,
  PpeDashboardKpi,
  PpeReportRow,
  PpeRequestDetail,
  PpeRequestListItem,
  PpeRequestPriority,
  PpeRequestStatus,
} from '../types/ppe';

export type PpeRequestFilter = {
  employeeUserId?: string;
  department?: string;
  status?: PpeRequestStatus;
  priority?: PpeRequestPriority;
  ppeCatalogueItemId?: string;
  fromDate?: string;
  toDate?: string;
  archivedOnly?: boolean;
  issuedOnly?: boolean;
};

export const ppeApi = {
  getDashboardKpis: () => unwrap<PpeDashboardKpi>(apiClient.get('/ppe/dashboard/kpis')),
  getCatalogue: (activeOnly = false) =>
    unwrap<PpeCatalogueItem[]>(apiClient.get('/ppe/catalogue', { params: { activeOnly } })),
  createCatalogueItem: (payload: Record<string, unknown>) =>
    unwrap<PpeCatalogueItem>(apiClient.post('/ppe/catalogue', payload)),
  updateCatalogueItem: (id: string, payload: Record<string, unknown>) =>
    unwrap<PpeCatalogueItem>(apiClient.put(`/ppe/catalogue/${id}`, payload)),
  getRequests: (filter?: PpeRequestFilter) =>
    unwrap<PpeRequestListItem[]>(apiClient.get('/ppe/requests', { params: filter ?? {} })),
  getRequest: (id: string) => unwrap<PpeRequestDetail>(apiClient.get(`/ppe/requests/${id}`)),
  createRequest: (payload: Record<string, unknown>) =>
    unwrap<PpeRequestDetail>(apiClient.post('/ppe/requests', payload)),
  updateRequest: (id: string, payload: Record<string, unknown>) =>
    unwrap<PpeRequestDetail>(apiClient.put(`/ppe/requests/${id}`, payload)),
  submitRequest: (id: string) => unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/submit`)),
  approveRequest: (id: string) => unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/approve`)),
  rejectRequest: (id: string, reason: string) =>
    unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/reject`, { reason })),
  dispatchRequest: (id: string, payload: Record<string, unknown>) =>
    unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/dispatch`, payload)),
  collectRequest: (id: string, collectedDate: string) =>
    unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/collect`, { collectedDate })),
  completeRequest: (id: string) => unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/complete`)),
  archiveRequest: (id: string) => unwrap<PpeRequestDetail>(apiClient.post(`/ppe/requests/${id}/archive`)),
  getEmployeeHistory: (employeeUserId: string) =>
    unwrap<EmployeePpeHistory[]>(apiClient.get(`/ppe/requests/employee/${employeeUserId}/history`)),
  reportOutstanding: () => unwrap<PpeRequestListItem[]>(apiClient.get('/ppe/reports/outstanding')),
  reportIssued: () => unwrap<PpeRequestListItem[]>(apiClient.get('/ppe/reports/issued-register')),
  reportOverdue: () => unwrap<PpeRequestListItem[]>(apiClient.get('/ppe/reports/overdue')),
  reportByDepartment: () => unwrap<PpeReportRow[]>(apiClient.get('/ppe/reports/by-department')),
  reportByEmployee: () => unwrap<PpeReportRow[]>(apiClient.get('/ppe/reports/by-employee')),
  reportByItem: () => unwrap<PpeReportRow[]>(apiClient.get('/ppe/reports/by-item')),
  reportUsageTrends: () => unwrap<PpeReportRow[]>(apiClient.get('/ppe/reports/usage-trends')),
};
