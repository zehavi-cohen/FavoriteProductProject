import { inject } from '@angular/core';
import {
  patchState,
  signalStore,
  withMethods,
  withState
} from '@ngrx/signals';
import { Router } from '@angular/router';

import { AdminService } from '../../core/services/admin.service';
import { AuthStore } from '../../core/stores/auth.store';
import { AdminUser } from '../../models/admin-user.model';

type AdminUsersState = {
  users: AdminUser[];
  isLoading: boolean;
  errorMessage: string | null;
};

const initialState: AdminUsersState = {
  users: [],
  isLoading: false,
  errorMessage: null
};

export const AdminUsersStore = signalStore(
  withState(initialState),

  withMethods(store => {
    const adminService = inject(AdminService);
    const authStore  = inject(AuthStore);
    const router = inject(Router);

    return {
      loadUsers(): void {
        patchState(store, {
          isLoading: true,
          errorMessage: null
        });

        adminService.getUsers().subscribe({
          next: users => {
            patchState(store, {
              users,
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
                  : 'שגיאה בטעינת המשתמשים'
            });
          }
        });
      },

      loginAsUser(user: AdminUser): void {
        if (this.isAdmin(user) || !user.isActive) {
          return;
        }

        patchState(store, {
          errorMessage: null
        });

        adminService.loginAsUser(user.userId).subscribe({
          next: response => {
          authStore.startImpersonation(response);
          router.navigate(['/products']);
          },
          error: error => {
            console.error(error);

            patchState(store, {
              errorMessage: 'שגיאה בכניסה כמשתמש'
            });
          }
        });
      },

      getRolesText(user: AdminUser): string {
        if (!user.roles || user.roles.length === 0) {
          return '-';
        }

        return user.roles.join(', ');
      },

      isAdmin(user: AdminUser): boolean {
        return user.roles.includes('Admin');
      }
    };
  })
);