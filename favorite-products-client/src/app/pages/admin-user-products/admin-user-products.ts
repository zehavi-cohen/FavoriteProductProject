import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AdminService } from '../../core/services/admin.service';
import { Product } from '../../models/product.model';

import { ProductsTable } from '../products-table/products-table';


@Component({
  selector: 'app-admin-user-products',
  standalone: true,
  imports: [RouterLink, ProductsTable], 
  templateUrl: './admin-user-products.html',
  styleUrl: './admin-user-products.scss'
})
export class AdminUserProducts {
  private readonly adminService = inject(AdminService);
  private readonly route = inject(ActivatedRoute);

  userId = signal<number | null>(null);
  userName = signal<string | null>(null);
  products = signal<Product[]>([]);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

    ngOnInit(): void {
    const userIdParam = this.route.snapshot.paramMap.get('userId');
    const parsedUserId = Number(userIdParam);
    const userNameParam = this.route.snapshot.queryParamMap.get('userName');
    this.userName.set(userNameParam);

    if (!userIdParam || Number.isNaN(parsedUserId)) {
      this.errorMessage.set('מזהה משתמש לא תקין');
      return;
    }

    this.userId.set(parsedUserId);
    this.loadProducts(parsedUserId);
  }

  loadProducts(userId: number): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.adminService.getUserProducts(userId).subscribe({
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
        this.errorMessage.set('שגיאה בטעינת מוצרי המשתמש');
        this.isLoading.set(false);
      }
    });
  }

  toggleFavorite(product: Product, isFavorite: boolean): void {
    const currentUserId = this.userId();

    if (!currentUserId) {
      this.errorMessage.set('מזהה משתמש לא תקין');
      return;
    }

    const previousValue = product.isFavorite;

    this.products.update(products =>
      products.map(item =>
        item.productId === product.productId
          ? { ...item, isFavorite }
          : item
      )
    );

    this.adminService
      .setUserFavorite(currentUserId, product.productId, isFavorite)
      .subscribe({
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

          this.errorMessage.set('שגיאה בעדכון מוצר אהוב עבור המשתמש');
        }
      });
  }
  getTableTitle(): string {
    const id = this.userId();
    const name = this.userName();

    if (id && name) {
      return `טבלת המוצרים למשתמש ${id} - ${name}`;
    }

    if (id) {
      return `טבלת המוצרים למשתמש ${id}`;
    }

    return 'טבלת המוצרים למשתמש';
  }
}