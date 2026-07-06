import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Cookie-auth replacement for the bearer interceptor: on a 401 from the API, send the browser to the
// server-rendered Identity login (a full-page navigation, carrying a ReturnUrl back to the current
// route). Other errors pass through untouched.
@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {
  public intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          const returnUrl = encodeURIComponent(
            window.location.pathname + window.location.search
          );
          window.location.assign(`/Identity/Account/Login?ReturnUrl=${returnUrl}`);
        }

        return throwError(() => error);
      })
    );
  }
}
