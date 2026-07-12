import {
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Box,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import AssignmentIcon from '@mui/icons-material/Assignment';
import ReportIcon from '@mui/icons-material/Report';
import LocalHospitalIcon from '@mui/icons-material/LocalHospital';
import PanToolIcon from '@mui/icons-material/PanTool';
import BuildIcon from '@mui/icons-material/Build';
import AssessmentIcon from '@mui/icons-material/Assessment';
import SettingsIcon from '@mui/icons-material/Settings';
import LogoutIcon from '@mui/icons-material/Logout';
import HealthAndSafetyIcon from '@mui/icons-material/HealthAndSafety';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import { NavLink, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../../auth/AuthProvider';

const DRAWER_WIDTH = 260;

const navItems = [
  { to: '/', label: 'Dashboard', icon: <DashboardIcon /> },
  { to: '/employees', label: 'Employees', icon: <PeopleIcon /> },
  { to: '/jsas', label: 'Job Safety Assessments', icon: <AssignmentIcon /> },
  { to: '/near-misses', label: 'Near Misses', icon: <ReportIcon /> },
  { to: '/injuries', label: 'Injuries', icon: <LocalHospitalIcon /> },
  { to: '/stop-unsafe-work', label: 'Stop Unsafe Work', icon: <PanToolIcon /> },
];

const ppeNavItems = [
  { to: '/ppe', label: 'Dashboard', end: true },
  { to: '/ppe/requests', label: 'PPE Requests' },
  { to: '/ppe/catalogue', label: 'PPE Catalogue' },
  { to: '/ppe/issued', label: 'Issued PPE' },
  { to: '/ppe/archived', label: 'Archived Requests' },
  { to: '/ppe/reports', label: 'Reports' },
];

const footerNavItems = [
  { to: '/corrective-actions', label: 'Corrective Actions', icon: <BuildIcon /> },
  { to: '/reports', label: 'Reports', icon: <AssessmentIcon /> },
  { to: '/settings', label: 'Settings', icon: <SettingsIcon /> },
];

interface SidebarProps {
  mobileOpen: boolean;
  onClose: () => void;
}

export default function Sidebar({ mobileOpen, onClose }: SidebarProps) {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [ppeOpen, setPpeOpen] = useState(true);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const drawer = (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Toolbar sx={{ px: 2 }}>
        <Box>
          <Box sx={{ fontWeight: 800, fontSize: 18, color: 'primary.main' }}>Job Safety Pro</Box>
          <Box sx={{ fontSize: 12, color: 'text.secondary' }}>Safety Manager Portal</Box>
        </Box>
      </Toolbar>
      <List sx={{ flex: 1, px: 1 }}>
        {navItems.map((item) => (
          <ListItemButton
            key={item.to}
            component={NavLink}
            to={item.to}
            end={item.to === '/'}
            onClick={isMobile ? onClose : undefined}
            sx={{
              borderRadius: 2,
              mb: 0.5,
              '&.active': {
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
                '& .MuiListItemIcon-root': { color: 'inherit' },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} primaryTypographyProps={{ fontSize: 14 }} />
          </ListItemButton>
        ))}
        <ListItemButton onClick={() => setPpeOpen((o) => !o)} sx={{ borderRadius: 2, mb: 0.5 }}>
          <ListItemIcon sx={{ minWidth: 40 }}><HealthAndSafetyIcon /></ListItemIcon>
          <ListItemText primary="PPE Management" primaryTypographyProps={{ fontSize: 14 }} />
          {ppeOpen ? <ExpandLess /> : <ExpandMore />}
        </ListItemButton>
        {ppeOpen && ppeNavItems.map((item) => (
          <ListItemButton
            key={item.to}
            component={NavLink}
            to={item.to}
            end={Boolean(item.end)}
            onClick={isMobile ? onClose : undefined}
            sx={{ borderRadius: 2, mb: 0.5, pl: 4, '&.active': { bgcolor: 'action.selected' } }}
          >
            <ListItemText primary={item.label} primaryTypographyProps={{ fontSize: 13 }} />
          </ListItemButton>
        ))}
        {footerNavItems.map((item) => (
          <ListItemButton
            key={item.to}
            component={NavLink}
            to={item.to}
            onClick={isMobile ? onClose : undefined}
            sx={{
              borderRadius: 2,
              mb: 0.5,
              '&.active': {
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
                '& .MuiListItemIcon-root': { color: 'inherit' },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} primaryTypographyProps={{ fontSize: 14 }} />
          </ListItemButton>
        ))}
      </List>
      <List sx={{ px: 1, pb: 2 }}>
        <ListItemButton onClick={handleLogout} sx={{ borderRadius: 2 }}>
          <ListItemIcon sx={{ minWidth: 40 }}>
            <LogoutIcon />
          </ListItemIcon>
          <ListItemText primary="Logout" />
        </ListItemButton>
      </List>
    </Box>
  );

  return (
    <Box component="nav" sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}>
      <Drawer
        variant={isMobile ? 'temporary' : 'permanent'}
        open={isMobile ? mobileOpen : true}
        onClose={onClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            borderRight: '1px solid',
            borderColor: 'divider',
          },
        }}
      >
        {drawer}
      </Drawer>
    </Box>
  );
}

export { DRAWER_WIDTH };
