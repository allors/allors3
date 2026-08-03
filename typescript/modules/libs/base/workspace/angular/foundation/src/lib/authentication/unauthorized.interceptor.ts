import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { NEVER, Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

const loginPath = '/Identity/Account/Login';

// Cookie-auth replacement for the bearer interceptor: on a 401 from the API, send the browser to the
// server-rendered Identity login (a full-page navigation, carrying a ReturnUrl back to the current
// route). Other errors pass through untouched.
@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {
  // Static, not per-instance: a bootstrap fires UserInfo and the first pulls together, so several
  // 401s land before the navigation commits, and the latch has to hold across every interceptor
  // instance an app happens to build.
  private static redirecting = false;

  public intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          if (
            !UnauthorizedInterceptor.redirecting &&
            !window.location.pathname.startsWith(loginPath)
          ) {
            UnauthorizedInterceptor.redirecting = true;

            const returnUrl = encodeURIComponent(
              window.location.pathname + window.location.search
            );
            window.location.assign(`${loginPath}?ReturnUrl=${returnUrl}`);
          }

          // Neither emit nor fail: this document is being replaced. Rethrowing surfaces the 401 to
          // whatever subscribed, and completing empty rejects firstValueFrom with an EmptyError —
          // either one lets a caller's own error handling start a second navigation, which cancels
          // the one above and leaves the app reloading in a loop.
          return NEVER;
        }

        return throwError(() => error);
      })
    );
  }
}
