import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthStore } from '../stores/auth.store';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  const token = authStore.token();

  const isAuthRequest =
    req.url.includes('/api/auth/login') ||
    req.url.includes('/api/auth/register');

  if (token && !isValidJwtFormat(token) && !isAuthRequest) {
    authStore.logout();

    queueMicrotask(() => {
      router.navigate(['/login'], {
        replaceUrl: true
      });
    });

    return throwError(() => new HttpErrorResponse({
      status: 401,
      statusText: 'Invalid token format',
      url: req.url
    }));
  }

  const requestToSend = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : req;

  return next(requestToSend).pipe(
    catchError((error: HttpErrorResponse) => {
      if (isAuthRequest) {
        return throwError(() => error);
      }

      if (error.status === 401) {
        authStore.logout();

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

function isValidJwtFormat(token: string): boolean {
  return /^[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+$/.test(token);
}