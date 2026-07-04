import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Box,
  Card,
  Chip,
  CircularProgress,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Typography,
} from '@mui/material';
import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import { workflowApi } from '../api/workflowApi';

export default function NotificationsPage() {
  const queryClient = useQueryClient();
  const { data, isLoading, error } = useQuery({
    queryKey: ['notifications'],
    queryFn: () => workflowApi.getNotifications(),
  });

  const markRead = useMutation({
    mutationFn: (id: string) => workflowApi.markRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-count'] });
    },
  });

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Notifications</Typography>
      {isLoading && <CircularProgress />}
      {error && <Alert severity="error">{(error as Error).message}</Alert>}
      <Card>
        <List>
          {(data ?? []).length === 0 && !isLoading && (
            <ListItem><ListItemText primary="No notifications." /></ListItem>
          )}
          {(data ?? []).map((n) => (
            <ListItem
              key={n.id}
              secondaryAction={
                !n.isRead && (
                  <IconButton edge="end" onClick={() => markRead.mutate(n.id)}>
                    <MarkEmailReadIcon />
                  </IconButton>
                )
              }
              sx={{ bgcolor: n.isRead ? 'transparent' : 'action.hover' }}
            >
              <ListItemText
                primary={
                  <Box display="flex" alignItems="center" gap={1}>
                    {n.title}
                    {!n.isRead && <Chip label="New" size="small" color="primary" />}
                  </Box>
                }
                secondary={
                  <>
                    <Typography variant="body2">{n.message}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {new Date(n.createdDate).toLocaleString()} · {n.notificationType}
                    </Typography>
                  </>
                }
              />
            </ListItem>
          ))}
        </List>
      </Card>
    </Box>
  );
}
