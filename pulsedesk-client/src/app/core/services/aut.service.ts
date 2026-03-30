import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserModel,
} from '../models/auth.models';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { ApiResponse } from '../models/api.models';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'pulsedesk_token';
  private readonly USER_KEY = 'pulsedesk_user';
  private readonly REFRESH_KEY = 'pulsedesk_refresh';

  private readonly currentUserSubject = new BehaviorSubject<UserModel | null>(
    this.getUserFromStorage(),
  );
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
  ) {}
  get currentUser(): UserModel | null {
    return this.currentUserSubject.value;
  }
  get isAuthenticated(): boolean {
    return !!this.getToken() && !!this.currentUser;
  }
  get isAdmin(): boolean {
    return this.currentUser?.role == 'Admin';
  }
  get isAdminOrManager(): boolean {
    return ['Admin', 'Manager'].includes(this.currentUser?.role ?? '');
  }
  hasPolicy(policy: string): boolean {
    return this.currentUser?.policies?.includes(policy) ?? false;
  }
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<
        ApiResponse<AuthResponse>
      >(`${environment.apiUrl}/Auth/login`, request)
      .pipe(
        tap((response) => {
          if (response.success) {
            this.setSession(response.data);
          }
        }),
      );
  }

  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<
        ApiResponse<AuthResponse>
      >(`${environment.apiUrl}/Auth/register`, request)
      .pipe(
        tap((response) => {
          if (response?.success) this.setSession(response?.data);
        }),
      );
  }
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }
  private setSession(authResponse: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, authResponse.accessToken);
    localStorage.setItem(this.REFRESH_KEY, authResponse.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(authResponse.user));
    this.currentUserSubject.next(authResponse.user);
  }
  private getUserFromStorage(): UserModel | null {
    const user = localStorage.getItem(this.USER_KEY);
    return user ? JSON.parse(user) : null;
  }
}
