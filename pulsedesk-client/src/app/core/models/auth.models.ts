export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  department?: string;
  jobTitle?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserModel;
}

export interface UserModel {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  role: string;
  policies: string[];
  department?: string;
  jobTitle?: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}
