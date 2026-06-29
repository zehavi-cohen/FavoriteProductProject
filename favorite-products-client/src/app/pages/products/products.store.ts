import { inject } from '@angular/core';
import {
  patchState,
  signalStore,
  withMethods,
  withState
} from '@ngrx/signals';

import { ProductService } from '../../core/services/product.service';
import { Product } from '../../models/product.model';

type ProductsState = {
  products: Product[];
  isLoading: boolean;
  errorMessage: string | null;
};

const initialState: ProductsState = {
  products: [],
  isLoading: false,
  errorMessage: null
};

export const ProductsStore = signalStore(
  withState(initialState),

  withMethods(store => {
    const productService = inject(ProductService);

    return {
      loadProducts(): void {
        patchState(store, {
          isLoading: true,
          errorMessage: null
        });

        productService.getMyProducts().subscribe({
          next: products => {
            patchState(store, {
              products,
              isLoading: false
            });
          },
          error: error => {
            console.error(error);

            patchState(store, {
              isLoading: false,
              errorMessage:
                error.status === 401 || error.status === 403
                  ? null
                  : 'שגיאה בטעינת המוצרים'
            });
          }
        });
      },

      toggleFavorite(product: Product, isFavorite: boolean): void {
        const previousValue = product.isFavorite;

        patchState(store, state => ({
          products: state.products.map(item =>
            item.productId === product.productId
              ? { ...item, isFavorite }
              : item
          ),
          errorMessage: null
        }));

        productService.setFavorite(product.productId, isFavorite).subscribe({
          next: () => {},
          error: error => {
            console.error(error);

            patchState(store, state => ({
              products: state.products.map(item =>
                item.productId === product.productId
                  ? { ...item, isFavorite: previousValue }
                  : item
              ),
              errorMessage:
                error.status === 401 || error.status === 403
                  ? null
                  : 'שגיאה בעדכון מוצר אהוב'
            }));
          }
        });
      }
    };
  })
);