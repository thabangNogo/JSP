import { useState } from 'react';
import { Navigate, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  IconButton,
  InputAdornment,
  Link,
  TextField,
  Typography,
  Alert,
} from '@mui/material';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import HealthAndSafetyIcon from '@mui/icons-material/HealthAndSafety';
import { useAuth } from '../auth/AuthProvider';
import axios from 'axios';

export default function LoginPage() {
  const { login, isLoading, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname ?? '/';

  const [email, setEmail] = useState('admin@jsp.demo');
  const [password, setPassword] = useState('Admin@123');
  const [remember, setRemember] = useState(true);
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');

  if (isAuthenticated) {
    return <Navigate to={from} replace />;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email.trim()) {
      setError('Email is required.');
      return;
    }
    if (!email.includes('@')) {
      setError('Enter a valid email address.');
      return;
    }
    if (password.length < 6) {
      setError('Password must be at least 6 characters.');
      return;
    }

    try {
      await login({ email: email.trim(), password }, remember);
      navigate(from, { replace: true });
    } catch (err) {
      if (axios.isAxiosError(err)) {
        const msg = typeof err.response?.data === 'string'
          ? err.response.data
          : (err.response?.data as { message?: string })?.message;
        setError(msg ?? 'Login failed. Check your credentials.');
      } else {
        setError(err instanceof Error ? err.message : 'Login failed.');
      }
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: 'background.default',
        p: 2,
      }}
    >
      <Card sx={{ width: '100%', maxWidth: 440 }}>
        <CardContent sx={{ p: 4 }}>
          <Box textAlign="center" mb={3}>
            <HealthAndSafetyIcon color="primary" sx={{ fontSize: 48, mb: 1 }} />
            <Typography variant="h5" fontWeight={700}>
              Safety Login
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Job Safety Pro
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              fullWidth
              label="Email Address"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              margin="normal"
              autoComplete="email"
            />
            <TextField
              fullWidth
              label="Password"
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              margin="normal"
              autoComplete="current-password"
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword((v) => !v)} edge="end">
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <Box display="flex" justifyContent="space-between" alignItems="center" mt={1}>
              <FormControlLabel
                control={
                  <Checkbox checked={remember} onChange={(e) => setRemember(e.target.checked)} />
                }
                label="Remember me"
              />
              <Link component="button" type="button" variant="body2" underline="hover" disabled>
                Forgot password?
              </Link>
            </Box>
            <Button
              fullWidth
              type="submit"
              variant="contained"
              size="large"
              sx={{ mt: 3 }}
              disabled={isLoading}
            >
              {isLoading ? <CircularProgress size={24} color="inherit" /> : 'Sign In'}
            </Button>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
