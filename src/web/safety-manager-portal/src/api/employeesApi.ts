import { apiClient, unwrap } from './axiosClient';
import type { PaginatedList } from '../types/api';
import type {
  EmployeeDetail,
  EmployeeListItem,
  EmployeeSearchParams,
  EmployeeStats,
} from '../types/employee';

import type { CreateEmployeePayload } from '../types/safety';

export const employeesApi = {
  search: (params: EmployeeSearchParams) =>
    unwrap<PaginatedList<EmployeeListItem>>(
      apiClient.get('/employees/search', { params }),
    ),

  getById: (id: string) => unwrap<EmployeeDetail>(apiClient.get(`/employees/${id}`)),

  getStats: () => unwrap<EmployeeStats>(apiClient.get('/employees/stats')),

  create: (payload: CreateEmployeePayload) =>
    unwrap<EmployeeListItem>(apiClient.post('/employees', payload)),
};
