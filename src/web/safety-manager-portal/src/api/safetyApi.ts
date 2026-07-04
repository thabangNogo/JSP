import { apiClient, unwrap } from './axiosClient';
import type {
  JobSafetyAssessment,
  NearMiss,
  StopUnsafeWorkReport,
  WorkLookups,
} from '../types/safety';

export type ManagerKpi = {
  nearMissesThisMonth: number;
  openNearMissInvestigations: number;
  stopUnsafeWorkReports: number;
  openCorrectiveActions: number;
  pendingAssessments: number;
  injuryFreeDays: number;
  openNearMisses: number;
  assessmentsSubmittedToday: number;
  totalEmployees: number;
  lostTimeInjuries: number;
  medicalTreatmentInjuries: number;
  firstAidInjuries: number;
  nearMissTrend: { label: string; value: number }[];
};

export const safetyApi = {
  getManagerKpis: () => unwrap<ManagerKpi>(apiClient.get('/safety-kpis/manager')),
  getInjuryFreeDays: () =>
    unwrap<{ injuryFreeDays: number }>(apiClient.get('/dashboard/injury-free-days')),
  getInjuries: () => unwrap<Record<string, unknown>[]>(apiClient.get('/injuries')),
  getInjuryById: (id: string) => unwrap<Record<string, unknown>>(apiClient.get(`/injuries/${id}`)),
  getWorkLookups: () => unwrap<WorkLookups>(apiClient.get('/work-lookups')),
  getJsaReports: (params?: { status?: string; department?: string; location?: string; section?: string }) =>
    unwrap<JobSafetyAssessment[]>(apiClient.get('/jsas/reports', { params })),
  getNearMisses: () => unwrap<NearMiss[]>(apiClient.get('/near-misses')),
  getStopUnsafeWork: () => unwrap<StopUnsafeWorkReport[]>(apiClient.get('/stop-unsafe-work')),
};
