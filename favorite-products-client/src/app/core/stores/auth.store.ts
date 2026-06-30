import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { catchError, EMPTY, Observable, tap } from 'rxjs';

import { AuthResponse } from '../../models/auth-response.model';
import { LoginRequest } from '../../models/login-request.model';
import { AuthApiService } from '../services/auth-api.service';

const authStorageKey = 'favorite_products_auth';
const originalAdminStorageKey = 'favorite_products_original_admin_auth';

type AuthState = {
  currentUser: AuthResponse | null;
  isLoading: boolean;
  error: string | null;
};

function readAuthFromStorage(): AuthResponse | null {
  const authJson = localStorage.getItem(authStorageKey);

  if (!authJson) {
    return null;
  }

  try {
    return JSON.parse(authJson) as AuthResponse;
  } catch {
    localStorage.removeItem(authStorageKey);
    return null;
  }
}

function saveAuthToStorage(response: AuthResponse): void {
  localStorage.setItem(authStorageKey, JSON.stringify(response));
}

const initialState: AuthState = {
  currentUser: readAuthFromStorage(),
  isLoading: false,
  error: null
};

export const AuthStore = signalStore(
  { providedIn: 'root' },

  withState(initialState),

  withComputed(store => ({
    isLoggedIn: computed(() => !!store.currentUser()),

    token: computed(() => store.currentUser()?.token ?? null),

    isAdmin: computed(() =>
      store.currentUser()?.roles?.includes('Admin') ?? false
    ),

    isImpersonating: computed(() =>
      store.currentUser()?.isImpersonating ?? false
    )
  })),

  withMethods((store, authApiService = inject(AuthApiService)) => {
    const applyAuth = (response: AuthResponse): void => {
      saveAuthToStorage(response);

      patchState(store, {
        currentUser: response,
        isLoading: false,
        error: null
      });
    };

    return {
      login(request: LoginRequest): Observable<AuthResponse> {
        patchState(store, {
          isLoading: true,
          error: null
        });

        return authApiService.login(request).pipe(
          tap(response => {
            applyAuth(response);
          }),
          catchError(error => {
            console.error(error);

            patchState(store, {
              isLoading: false,
              error: 'שם משתמש או סיסמה שגויים'
            });

            return EMPTY;
          })
        );
      },

      setAuth(response: AuthResponse): void {
        applyAuth(response);
      },

      clearError(): void {
        patchState(store, {
          error: null
        });
      },

      logout(): void {
        localStorage.removeItem(authStorageKey);
        localStorage.removeItem(originalAdminStorageKey);

        patchState(store, {
          currentUser: null,
          isLoading: false,
          error: null
        });
      },

      startImpersonation(response: AuthResponse): void {
        const currentAdmin = store.currentUser();

        if (currentAdmin) {
          localStorage.setItem(
            originalAdminStorageKey,
            JSON.stringify(currentAdmin)
          );
        }

        applyAuth(response);
      },

      stopImpersonation(): boolean {
        const originalAdminJson = localStorage.getItem(originalAdminStorageKey);

        if (!originalAdminJson) {
          return false;
        }

        try {
          const originalAdmin = JSON.parse(originalAdminJson) as AuthResponse;

          localStorage.removeItem(originalAdminStorageKey);
          applyAuth(originalAdmin);

          return true;
        } catch {
          localStorage.removeItem(originalAdminStorageKey);
          return false;
        }
      }
    };
  })
);