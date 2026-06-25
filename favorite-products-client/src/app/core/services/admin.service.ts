import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { AdminUser } from '../../models/admin-user.model';
import { Product } from '../../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = 'https://localhost:7001';

  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<AdminUser[]>(`${this.apiUrl}/api/admin/users`);
  }

  getUserProducts(userId: number) {
    return this.http.get<Product[]>(
      `${this.apiUrl}/api/admin/users/${userId}/products`
    );
  }

  setUserFavorite(userId: number, productId: number, isFavorite: boolean) {
    return this.http.put<void>(
      `${this.apiUrl}/api/admin/users/${userId}/products/${productId}/favorite`,
      { isFavorite }
    );
  }
}