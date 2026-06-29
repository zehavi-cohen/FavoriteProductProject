import { Component, inject, signal } from '@angular/core';

import { ProductService } from '../../core/services/product.service';
import { AuthService } from '../../core/services/auth.service';
import { Product } from '../../models/product.model';
import { ProductsTable } from '../products-table/products-table';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [ProductsTable],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products {
  private readonly productService = inject(ProductService);
  private readonly authService = inject(AuthService);

  products = signal<Product[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadProducts();
  }

  getTableTitle(): string {
    const currentUser = this.authService.currentUser();

    if (!currentUser) {
      return 'טבלת המוצרים למשתמש';
    }

    return `טבלת המוצרים שלי -  ${currentUser.userName}`;
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.productService.getMyProducts().subscribe({
      next: products => {
        this.products.set(products);
        this.isLoading.set(false);
      },
      error: error => {
        console.error(error);
          if (error.status === 401 || error.status === 403) {
            this.isLoading.set(false);
            return;
          }
        this.errorMessage.set('שגיאה בטעינת המוצרים');
        this.isLoading.set(false);
      }
    });
  }

  toggleFavorite(product: Product, isFavorite: boolean): void {
    const previousValue = product.isFavorite;

    this.products.update(products =>
      products.map(item =>
        item.productId === product.productId
          ? { ...item, isFavorite }
          : item
      )
    );

    this.productService.setFavorite(product.productId, isFavorite).subscribe({
      next: () => {},
      error: error => {
        console.error(error);
        
          if (error.status === 401 || error.status === 403) {
            return;
          }
          
        this.products.update(products =>
          products.map(item =>
            item.productId === product.productId
              ? { ...item, isFavorite: previousValue }
              : item
          )
        );

        this.errorMessage.set('שגיאה בעדכון מוצר אהוב');
      }
    });
  }
}