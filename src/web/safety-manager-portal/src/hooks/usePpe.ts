import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { ppeApi, type PpeRequestFilter } from '../api/ppeApi';

export function usePpeDashboardKpis() {
  return useQuery({ queryKey: ['ppe', 'dashboard'], queryFn: () => ppeApi.getDashboardKpis() });
}

export function usePpeCatalogue(activeOnly = false) {
  return useQuery({ queryKey: ['ppe', 'catalogue', activeOnly], queryFn: () => ppeApi.getCatalogue(activeOnly) });
}

export function usePpeRequests(filter?: PpeRequestFilter) {
  return useQuery({
    queryKey: ['ppe', 'requests', filter],
    queryFn: () => ppeApi.getRequests(filter),
  });
}

export function usePpeRequest(id?: string) {
  return useQuery({
    queryKey: ['ppe', 'request', id],
    queryFn: () => ppeApi.getRequest(id!),
    enabled: Boolean(id),
  });
}

export function useEmployeePpeHistory(employeeUserId?: string) {
  return useQuery({
    queryKey: ['ppe', 'employee-history', employeeUserId],
    queryFn: () => ppeApi.getEmployeeHistory(employeeUserId!),
    enabled: Boolean(employeeUserId),
  });
}

export function usePpeMutations() {
  const queryClient = useQueryClient();
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['ppe'] });
  };

  return {
    createRequest: useMutation({ mutationFn: ppeApi.createRequest, onSuccess: invalidate }),
    updateRequest: useMutation({
      mutationFn: ({ id, payload }: { id: string; payload: Record<string, unknown> }) =>
        ppeApi.updateRequest(id, payload),
      onSuccess: invalidate,
    }),
    approveRequest: useMutation({ mutationFn: ppeApi.approveRequest, onSuccess: invalidate }),
    rejectRequest: useMutation({
      mutationFn: ({ id, reason }: { id: string; reason: string }) => ppeApi.rejectRequest(id, reason),
      onSuccess: invalidate,
    }),
    dispatchRequest: useMutation({
      mutationFn: ({ id, payload }: { id: string; payload: Record<string, unknown> }) =>
        ppeApi.dispatchRequest(id, payload),
      onSuccess: invalidate,
    }),
    collectRequest: useMutation({
      mutationFn: ({ id, collectedDate }: { id: string; collectedDate: string }) =>
        ppeApi.collectRequest(id, collectedDate),
      onSuccess: invalidate,
    }),
    completeRequest: useMutation({ mutationFn: ppeApi.completeRequest, onSuccess: invalidate }),
    archiveRequest: useMutation({ mutationFn: ppeApi.archiveRequest, onSuccess: invalidate }),
    createCatalogueItem: useMutation({ mutationFn: ppeApi.createCatalogueItem, onSuccess: invalidate }),
    updateCatalogueItem: useMutation({
      mutationFn: ({ id, payload }: { id: string; payload: Record<string, unknown> }) =>
        ppeApi.updateCatalogueItem(id, payload),
      onSuccess: invalidate,
    }),
  };
}
