import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { AdminUser } from '../../models/admin-user.model';
import { AuthResponse } from '../../models/auth-response.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = 'https://localhost:7001';

  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<AdminUser[]>(`${this.apiUrl}/api/admin/users`);
  }

  loginAsUser(userId: number) {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/api/admin/users/${userId}/login-as`,
      {}
    );
  }

}