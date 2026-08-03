import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
// Imported by path rather than through the '@allors/base/workspace/angular/foundation' barrel on
// purpose: the barrel re-exports fields that depend on '@allors/default/workspace/*', which only
// exist after a .NET `Generate` run. The interceptor itself needs nothing but Angular and rxjs, so
// the deep import keeps this suite runnable from a bare checkout.
// eslint-disable-next-line @nrwl/nx/enforce-module-boundaries
import { UnauthorizedInterceptor } from '../../../../foundation/src/lib/authentication/unauthorized.interceptor';
import { Observable, firstValueFrom, of, throwError } from 'rxjs';

class FakeHandler implements HttpHandler {
  constructor(private readonly response: Observable<HttpEvent<unknown>>) {}

  handle(): Observable<HttpEvent<unknown>> {
    return this.response;
  }
}

const failWith = (status: number, url: string) =>
  new FakeHandler(throwError(() => new HttpErrorResponse({ status, url })));

describe('UnauthorizedInterceptor', () => {
  const request = new HttpRequest('GET', '/allors/UserInfo');

  let interceptor: UnauthorizedInterceptor;
  let location: { pathname: string; search: string; assign: jest.Mock };
  let originalLocation: Location;

  beforeEach(() => {
    originalLocation = window.location;
    location = { pathname: '/dashboard', search: '', assign: jest.fn() };
    Object.defineProperty(window, 'location', {
      value: location,
      writable: true,
      configurable: true,
    });

    // The redirect latch is static: a single interceptor instance serves the whole app, but the
    // latch must also survive across instances, so it has to be cleared explicitly per test.
    (
      UnauthorizedInterceptor as unknown as { redirecting: boolean }
    ).redirecting = false;

    interceptor = new UnauthorizedInterceptor();
  });

  afterEach(() => {
    Object.defineProperty(window, 'location', {
      value: originalLocation,
      writable: true,
      configurable: true,
    });
  });

  it('redirects a 401 to the Identity login, carrying the current route as ReturnUrl', () => {
    location.pathname = '/dashboard';
    location.search = '?q=1';

    interceptor.intercept(request, failWith(401, '/allors/UserInfo')).subscribe();

    expect(location.assign).toHaveBeenCalledTimes(1);
    expect(location.assign).toHaveBeenCalledWith(
      '/Identity/Account/Login?ReturnUrl=%2Fdashboard%3Fq%3D1'
    );
  });

  // The Safari/WebKit login loop: a second 401 landing in the same tick used to restart the
  // navigation, dropping the ReturnUrl captured by the first one.
  it('redirects only once when several 401s land together', () => {
    interceptor.intercept(request, failWith(401, '/allors/UserInfo')).subscribe();
    location.pathname = '/Identity/Account/Login';
    interceptor.intercept(request, failWith(401, '/allors/pull')).subscribe();
    interceptor.intercept(request, failWith(401, '/allors/pull')).subscribe();

    expect(location.assign).toHaveBeenCalledTimes(1);
    expect(location.assign).toHaveBeenCalledWith(
      '/Identity/Account/Login?ReturnUrl=%2Fdashboard'
    );
  });

  it('does not redirect when already on the login page', () => {
    location.pathname = '/Identity/Account/Login';
    location.search = '?ReturnUrl=%2Fdashboard';

    interceptor.intercept(request, failWith(401, '/allors/UserInfo')).subscribe();

    expect(location.assign).not.toHaveBeenCalled();
  });

  // Returning EMPTY here would make firstValueFrom reject with EmptyError, which is not an
  // HttpErrorResponse — so the app's bootstrap fallback would treat it as "server unreachable"
  // and navigate to /error, cancelling the login navigation we just started. NEVER lets the
  // pending navigation win.
  it('neither emits, completes nor errors on a 401, so callers simply await the navigation', async () => {
    let settled: string | null = null;

    interceptor.intercept(request, failWith(401, '/allors/UserInfo')).subscribe({
      next: () => (settled = 'next'),
      error: () => (settled = 'error'),
      complete: () => (settled = 'complete'),
    });

    await new Promise((resolve) => setTimeout(resolve, 10));

    expect(settled).toBeNull();
    expect(location.assign).toHaveBeenCalledTimes(1);
  });

  it('propagates non-401 errors untouched', async () => {
    const error = await firstValueFrom(
      interceptor.intercept(request, failWith(500, '/allors/pull'))
    ).catch((e: unknown) => e);

    expect(error).toBeInstanceOf(HttpErrorResponse);
    expect((error as HttpErrorResponse).status).toBe(500);
    expect(location.assign).not.toHaveBeenCalled();
  });

  it('passes successful responses through', async () => {
    const response = new HttpResponse({ status: 200, body: { u: '1' } });

    const result = await firstValueFrom(
      interceptor.intercept(request, new FakeHandler(of(response)))
    );

    expect(result).toBe(response);
    expect(location.assign).not.toHaveBeenCalled();
  });
});
