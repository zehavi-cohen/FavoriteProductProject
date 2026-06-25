import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Products } from './pages/products/products';
import { AdminUsers } from './pages/admin-users/admin-users';
import { AdminUserProducts } from './pages/admin-user-products/admin-user-products';

export const routes: Routes = [
  {
    path: 'login',
    component: Login
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: 'products',
    component: Products,
    canActivate: [authGuard]
  },
  {
    path: 'admin/users',
    component: AdminUsers,
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/users/:userId/products',
    component: AdminUserProducts,
    canActivate: [authGuard, adminGuard]
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];