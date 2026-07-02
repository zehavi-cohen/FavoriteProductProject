import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthStore } from '../stores/auth.store';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  const isLoginOrRegisterRequest =
    req.url.includes('/api/auth/login') ||
    req.url.includes('/api/auth/register');

  const isMeRequest =
    req.url.includes('/api/auth/me');

  const requestToSend = req.clone({
    withCredentials: true
  });

  return next(requestToSend).pipe(
    catchError((error: HttpErrorResponse) => {
      if (isLoginOrRegisterRequest || isMeRequest) {
        return throwError(() => error);
      }

      if (error.status === 401) {
        authStore.clearAuth();

        queueMicrotask(() => {
          router.navigate(['/login'], {
            replaceUrl: true
          });
        });
      }

      if (error.status === 403) {
        queueMicrotask(() => {
          router.navigate(['/products'], {
            replaceUrl: true
          });
        });
      }

      return throwError(() => error);
    })
  );
};