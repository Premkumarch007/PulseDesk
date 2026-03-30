import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ApiResponse, PagedResponse } from '../models/api.models';
import { UserListItem } from '../models/dashboard.models';
import { UserModel } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = `${environment.apiUrl}/Users`;
  constructor(private readonly http: HttpClient) {}

  getUsers(
    request: UserListRequest = {},
  ): Observable<ApiResponse<PagedResponse<UserListItem>>> {
    let params = new HttpParams();
    if (request.search) params = params.set('search', request.search);
    if (request.role) params = params.set('role', request.role);
    if (request.isActive != undefined)
      params = params.set('isActive', request.isActive.toString());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection)
      params = params.set('sortDirection', request.sortDirection);

    params = params
      .set('page', (request.page ?? 1).toString())
      .set('pageSize', (request.pageSize ?? 10).toString());

    return this.http.get<ApiResponse<PagedResponse<UserListItem>>>(
      this.baseUrl,
      { params },
    );
  }

  getUserById(id: number): Observable<ApiResponse<UserModel>> {
    return this.http.get<ApiResponse<UserModel>>(`${this.baseUrl}/${id}`);
  }
  updateRole(
    userId: number,
    roleName: string,
  ): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.baseUrl}/${userId}/role`,
      { roleName },
    );
  }

  deactivateUser(userId: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.baseUrl}/${userId}/deactivate`,
      {},
    );
  }
}
export interface UserListRequest {
  search?: string;
  role?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  page?: number;
  pageSize?: number;
}
