import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { AuthResponse } from '../../models/auth-response.model';
import { LoginRequest } from '../../models/login-request.model';
import { RegisterRequest } from '../../models/register-request.model';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly apiUrl = 'https://localhost:7001';

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/api/auth/login`,
      request
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/api/auth/register`,
      request
    );
  }

  me(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(
      `${this.apiUrl}/api/auth/me`
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/api/auth/logout`,
      {}
    );
  }

  stopImpersonation(): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(
    `${this.apiUrl}/api/admin/stop-impersonation`,
    {}
  );
}
}