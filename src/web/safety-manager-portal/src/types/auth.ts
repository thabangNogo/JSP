export interface AuthUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  companyId: string;
  plantId?: string;
  departmentId?: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: AuthUser;
}

export interface LoginRequest {
  email: string;
  password: string;
}
