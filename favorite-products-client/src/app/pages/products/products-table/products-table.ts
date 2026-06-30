import { Component, computed, input, output, signal } from '@angular/core';

import { Product } from '../../../models/product.model';

type ProductsFilter = 'all' | 'favorites' | 'notFavorites';

@Component({
  selector: 'app-products-table',
  standalone: true,
  imports: [],
  templateUrl: './products-table.html',
  styleUrl: './products-table.scss'
})
export class ProductsTable {
  products = input.required<Product[]>();

  searchText = signal('');
  filter = signal<ProductsFilter>('all');

  totalProducts = computed(() => this.products().length);
  
  favoriteProductsCount = computed(() =>
    this.products().filter(product => product.isFavorite).length
  );

  filteredProducts = computed(() => {
    const search = this.searchText().trim().toLowerCase();
    const filter = this.filter();

    return this.products().filter(product => {
      const matchesSearch =
        !search ||
        product.productName.toLowerCase().includes(search) ||
        (product.code ?? '').toLowerCase().includes(search) ||
        (product.description ?? '').toLowerCase().includes(search);

      const matchesFilter =
        filter === 'all' ||
        (filter === 'favorites' && product.isFavorite) ||
        (filter === 'notFavorites' && !product.isFavorite);

      return matchesSearch && matchesFilter;
    });
  });

  displayedProductsCount = computed(() => this.filteredProducts().length);

  favoriteChanged = output<{
    product: Product;
    isFavorite: boolean;
  }>();

  onSearchChanged(value: string): void {
    this.searchText.set(value);
  }

  onFilterChanged(value: string): void {
    if (
      value === 'all' ||
      value === 'favorites' ||
      value === 'notFavorites'
    ) {
      this.filter.set(value);
    }
  }

  clearFilters(): void {
    this.searchText.set('');
    this.filter.set('all');
  }

  onFavoriteChanged(product: Product, isFavorite: boolean): void {
    this.favoriteChanged.emit({
      product,
      isFavorite
    });
  }
}