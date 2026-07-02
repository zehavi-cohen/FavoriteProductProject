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

type AuthState = {
  currentUser: AuthResponse | null;
  isLoading: boolean;
  error: string | null;
};

const initialState: AuthState = {
  currentUser: null,
  isLoading: false,
  error: null
};

export const AuthStore = signalStore(
  { providedIn: 'root' },

  withState(initialState),

  withComputed(store => ({
    isLoggedIn: computed(() => !!store.currentUser()),

    isAdmin: computed(() =>
      store.currentUser()?.roles?.includes('Admin') ?? false
    ),

    isImpersonating: computed(() =>
      store.currentUser()?.isImpersonating ?? false
    )
  })),

  withMethods((store, authApiService = inject(AuthApiService)) => {
    const setCurrentUser = (response: AuthResponse): void => {
      patchState(store, {
        currentUser: response,
        isLoading: false,
        error: null
      });
    };

    const clearAuth = (): void => {
      patchState(store, {
        currentUser: null,
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
            setCurrentUser(response);
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

      loadCurrentUser(): Observable<AuthResponse> {
        patchState(store, {
          isLoading: true,
          error: null
        });

        return authApiService.me().pipe(
          tap(response => {
            setCurrentUser(response);
          }),
          catchError(() => {
            clearAuth();

            return EMPTY;
          })
        );
      },

      logout(): Observable<void> {
        patchState(store, {
          isLoading: true,
          error: null
        });

        return authApiService.logout().pipe(
          tap(() => {
            clearAuth();
          }),
          catchError(error => {
            console.error(error);

            clearAuth();

            return EMPTY;
          })
        );
      },

      clearAuth(): void {
        clearAuth();
      },

      setAuth(response: AuthResponse): void {
        setCurrentUser(response);
      },

      clearError(): void {
        patchState(store, {
          error: null
        });
      },

      startImpersonation(response: AuthResponse): void {
        setCurrentUser(response);
      },

      stopImpersonation(): Observable<AuthResponse> {
        patchState(store, {
          isLoading: true,
          error: null
        });

        return authApiService.stopImpersonation().pipe(
          tap(response => {
            setCurrentUser(response);
          }),
          catchError(error => {
            console.error(error);

            patchState(store, {
              isLoading: false,
              error: 'לא ניתן לחזור למשתמש המקורי'
            });

            return EMPTY;
          })
        );
      }
      
    };
  })
);