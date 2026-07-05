import { Component, inject } from '@angular/core';

import { ProductsTable } from '../products-table/products-table';
import { ProductsStore } from '../products.store';
@Component({
  selector: 'app-products',
  standalone: true,
  imports: [ProductsTable],
  providers: [ProductsStore],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products {
  readonly store = inject(ProductsStore);

  ngOnInit(): void {
    this.store.loadProducts();
  }
}