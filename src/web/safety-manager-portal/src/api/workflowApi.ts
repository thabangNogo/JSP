import { apiClient, unwrap } from './axiosClient';

export interface JsaReview {
  id: string;
  title: string;
  jobDescription: string;
  department: string;
  location: string;
  section: string;
  status: string;
  employeeName?: string;
  signOffName?: string;
  signOffSurname?: string;
  signOffCompanyNumber?: string;
  signOffOccupation?: string;
  signatureStoragePath?: string;
  rejectionReason?: string;
  hazards: { description: string }[];
  controlMeasures: { description: string }[];
}

export interface PendingActions {
  pendingJsaApprovals: number;
  nearMissesAwaitingInvestigation: number;
  correctiveActionsOverdue: number;
  openInjuries: number;
  openUnsafeWorkReports: number;
  pendingJsas: number;
  items: {
    module: string;
    id: string;
    title: string;
    status: string;
    createdDate: string;
  }[];
}

export interface SafetyNotification {
  id: string;
  title: string;
  message: string;
  priority: string;
  notificationType: string;
  relatedEntityType?: string;
  relatedEntityId?: string;
  isRead: boolean;
  createdDate: string;
}

export const workflowApi = {
  getPendingActions: () =>
    unwrap<PendingActions>(apiClient.get('/workflows/dashboard/pending-actions')),
  getPendingJsas: () =>
    unwrap<{ id: string; title: string; status: string; department: string }[]>(
      apiClient.get('/workflows/jsas/pending-approval'),
    ),
  getJsaReview: (id: string) =>
    unwrap<JsaReview>(apiClient.get(`/workflows/jsas/${id}/review`)),
  approveJsa: (id: string) =>
    unwrap<unknown>(apiClient.post(`/workflows/jsas/${id}/approve`)),
  rejectJsa: (id: string, rejectionReason: string) =>
    unwrap<unknown>(apiClient.post(`/workflows/jsas/${id}/reject`, { rejectionReason })),
  getNotifications: () =>
    unwrap<SafetyNotification[]>(apiClient.get('/notifications')),
  getUnreadCount: () =>
    unwrap<{ unreadCount: number }>(apiClient.get('/notifications/unread-count')),
  markRead: (id: string) =>
    unwrap<unknown>(apiClient.post(`/notifications/${id}/read`)),
};
