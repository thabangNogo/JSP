import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Typography,
} from '@mui/material';
import { useAuth } from '../../auth/AuthProvider';
import { usePpeMutations } from '../../hooks/usePpe';
import type { PpeRequestStatus } from '../../types/ppe';

type Props = {
  requestId: string;
  status: PpeRequestStatus;
  employeeName: string;
  variant?: 'inline' | 'panel';
  onActionComplete?: () => void;
};

function hasRole(roles: string[], names: string[]) {
  return roles.some((r) => names.includes(r));
}

export default function PpeRequestWorkflowActions({
  requestId,
  status,
  employeeName,
  variant = 'panel',
  onActionComplete,
}: Props) {
  const { user } = useAuth();
  const roles = user?.roles ?? [];
  const isManager = hasRole(roles, ['Safety Manager', 'HSE Manager', 'Administrator']);
  const isSafetyLead = hasRole(roles, [
    'Safety Manager',
    'Safety Officer',
    'HSE Manager',
    'Administrator',
  ]);

  const {
    approveRequest,
    rejectRequest,
    dispatchRequest,
    collectRequest,
    completeRequest,
    archiveRequest,
  } = usePpeMutations();

  const [rejectOpen, setRejectOpen] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [dispatchOpen, setDispatchOpen] = useState(false);
  const [dispatchForm, setDispatchForm] = useState({
    dispatchDate: new Date().toISOString().slice(0, 10),
    collectedByEmployee: employeeName,
    employeeSignature: '',
    safetyOfficerSignature: user ? `${user.firstName} ${user.lastName}`.trim() : '',
    notes: '',
  });

  const pending =
    approveRequest.isPending ||
    rejectRequest.isPending ||
    dispatchRequest.isPending ||
    collectRequest.isPending ||
    completeRequest.isPending ||
    archiveRequest.isPending;

  const mutationError =
    approveRequest.error ??
    rejectRequest.error ??
    dispatchRequest.error ??
    collectRequest.error ??
    completeRequest.error ??
    archiveRequest.error;

  const canApprove = isManager && (status === 'Requested' || status === 'PendingApproval');
  const canReject = isManager && (status === 'Requested' || status === 'PendingApproval');
  const canDispatch = isSafetyLead && (status === 'Preparing' || status === 'Approved');
  const canCollect = status === 'Dispatched';
  const canComplete = isSafetyLead && status === 'Collected';
  const canArchive = isSafetyLead && (status === 'Completed' || status === 'Rejected');

  const hasActions = canApprove || canReject || canDispatch || canCollect || canComplete || canArchive;

  if (!hasActions) {
    if (variant === 'panel') {
      return (
        <Alert severity="info" sx={{ mt: 2 }}>
          No workflow actions available for status: {status}.
        </Alert>
      );
    }
    return null;
  }

  const buttonSize = variant === 'inline' ? 'small' : 'medium';

  const run = (fn: () => void) => {
    fn();
    onActionComplete?.();
  };

  return (
    <>
      {variant === 'panel' && (
        <Typography variant="h6" gutterBottom>
          Workflow Actions
        </Typography>
      )}
      {mutationError && (
        <Alert severity="error" sx={{ mb: variant === 'panel' ? 2 : 1 }}>
          {(mutationError as Error).message}
        </Alert>
      )}
      <Box display="flex" gap={1} flexWrap="wrap">
        {canApprove && (
          <Button
            size={buttonSize}
            variant="contained"
            color="success"
            disabled={pending}
            onClick={() => run(() => approveRequest.mutate(requestId))}
          >
            Approve
          </Button>
        )}
        {canReject && (
          <Button
            size={buttonSize}
            variant="outlined"
            color="error"
            disabled={pending}
            onClick={() => setRejectOpen(true)}
          >
            Reject
          </Button>
        )}
        {canDispatch && (
          <Button
            size={buttonSize}
            variant="contained"
            disabled={pending}
            onClick={() => {
              setDispatchForm((f) => ({ ...f, collectedByEmployee: employeeName }));
              setDispatchOpen(true);
            }}
          >
            Dispatch
          </Button>
        )}
        {canCollect && (
          <Button
            size={buttonSize}
            variant="contained"
            disabled={pending}
            onClick={() =>
              run(() =>
                collectRequest.mutate({
                  id: requestId,
                  collectedDate: new Date().toISOString(),
                }),
              )
            }
          >
            Mark Collected
          </Button>
        )}
        {canComplete && (
          <Button
            size={buttonSize}
            variant="contained"
            color="secondary"
            disabled={pending}
            onClick={() => run(() => completeRequest.mutate(requestId))}
          >
            Complete
          </Button>
        )}
        {canArchive && (
          <Button
            size={buttonSize}
            variant="outlined"
            disabled={pending}
            onClick={() => run(() => archiveRequest.mutate(requestId))}
          >
            Archive
          </Button>
        )}
      </Box>

      <Dialog open={rejectOpen} onClose={() => setRejectOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Reject PPE Request</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            required
            multiline
            minRows={3}
            label="Rejection Reason"
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRejectOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color="error"
            disabled={!rejectReason.trim() || pending}
            onClick={() => {
              rejectRequest.mutate(
                { id: requestId, reason: rejectReason.trim() },
                {
                  onSuccess: () => {
                    setRejectOpen(false);
                    setRejectReason('');
                    onActionComplete?.();
                  },
                },
              );
            }}
          >
            Reject Request
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={dispatchOpen} onClose={() => setDispatchOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Dispatch PPE</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: 1 }}>
          <TextField
            label="Issued By"
            value={user ? `${user.firstName} ${user.lastName}`.trim() : ''}
            InputProps={{ readOnly: true }}
          />
          <TextField
            label="Dispatch Date"
            type="date"
            InputLabelProps={{ shrink: true }}
            value={dispatchForm.dispatchDate}
            onChange={(e) => setDispatchForm((f) => ({ ...f, dispatchDate: e.target.value }))}
          />
          <TextField
            label="Collected By Employee"
            value={dispatchForm.collectedByEmployee}
            onChange={(e) => setDispatchForm((f) => ({ ...f, collectedByEmployee: e.target.value }))}
          />
          <TextField
            label="Employee Signature"
            value={dispatchForm.employeeSignature}
            onChange={(e) => setDispatchForm((f) => ({ ...f, employeeSignature: e.target.value }))}
          />
          <TextField
            label="Safety Officer Signature"
            value={dispatchForm.safetyOfficerSignature}
            onChange={(e) => setDispatchForm((f) => ({ ...f, safetyOfficerSignature: e.target.value }))}
          />
          <TextField
            label="Notes"
            multiline
            minRows={2}
            value={dispatchForm.notes}
            onChange={(e) => setDispatchForm((f) => ({ ...f, notes: e.target.value }))}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDispatchOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            disabled={pending}
            onClick={() => {
              dispatchRequest.mutate(
                {
                  id: requestId,
                  payload: {
                    dispatchDate: dispatchForm.dispatchDate,
                    collectedByEmployee: dispatchForm.collectedByEmployee,
                    employeeSignature: dispatchForm.employeeSignature || null,
                    safetyOfficerSignature: dispatchForm.safetyOfficerSignature || null,
                    notes: dispatchForm.notes,
                  },
                },
                {
                  onSuccess: () => {
                    setDispatchOpen(false);
                    onActionComplete?.();
                  },
                },
              );
            }}
          >
            Dispatch
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
