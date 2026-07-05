import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Product } from '../../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly apiUrl = 'https://localhost:7001';

  constructor(private http: HttpClient) {}

  getMyProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/api/products/my`);
  }

  setFavorite(productId: number, isFavorite: boolean): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/api/products/${productId}/favorite`,
      { isFavorite }
    );
  }
}