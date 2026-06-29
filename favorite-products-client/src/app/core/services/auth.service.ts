import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';

import { AuthResponse } from '../../models/auth-response.model';
import { LoginRequest } from '../../models/login-request.model';
import { RegisterRequest } from '../../models/register-request.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:7001';
  private readonly storageKey = 'favorite_products_auth';
  private readonly originalAdminStorageKey = 'favorite_products_original_admin_auth';

  public currentUser = signal<AuthResponse | null>(this.getUserFromStorage());
  public originalAdminUser = signal<AuthResponse | null>(this.getOriginalAdminFromStorage());

  constructor(private http: HttpClient) {}

  register(request: RegisterRequest) {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/api/auth/register`, request)
      .pipe(tap(response => this.saveRegularAuth(response)));
  }

  login(request: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/api/auth/login`, request)
      .pipe(tap(response => this.saveRegularAuth(response)));
  }

  startImpersonation(response: AuthResponse): void {
    const currentUser = this.currentUser();

    if (currentUser && !currentUser.isImpersonating) {
      localStorage.setItem(
        this.originalAdminStorageKey,
        JSON.stringify(currentUser)
      );

      this.originalAdminUser.set(currentUser);
    }

    this.saveAuth(response);
  }

  stopImpersonation(): boolean {
    const originalAdmin = this.getOriginalAdminFromStorage();

    if (!originalAdmin) {
      return false;
    }

    localStorage.removeItem(this.originalAdminStorageKey);
    this.originalAdminUser.set(null);

    this.saveAuth(originalAdmin);

    return true;
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    localStorage.removeItem(this.originalAdminStorageKey);

    this.currentUser.set(null);
    this.originalAdminUser.set(null);
  }

  getToken(): string | null {
    return this.currentUser()?.token ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.currentUser()?.roles.includes('Admin') ?? false;
  }

  isImpersonating(): boolean {
    return this.currentUser()?.isImpersonating ?? false;
  }

  private saveRegularAuth(response: AuthResponse): void {
    localStorage.removeItem(this.originalAdminStorageKey);
    this.originalAdminUser.set(null);

    this.saveAuth(response);
  }

  private saveAuth(response: AuthResponse): void {
    localStorage.setItem(this.storageKey, JSON.stringify(response));
    this.currentUser.set(response);
  }

  private getUserFromStorage(): AuthResponse | null {
    const raw = localStorage.getItem(this.storageKey);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthResponse;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }

  private getOriginalAdminFromStorage(): AuthResponse | null {
    const raw = localStorage.getItem(this.originalAdminStorageKey);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as AuthResponse;
    } catch {
      localStorage.removeItem(this.originalAdminStorageKey);
      return null;
    }
  }
}