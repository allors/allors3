import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

// Posts to the server-rendered Identity logout (Angular attaches the X-XSRF-TOKEN header on this
// same-origin mutating request), then performs a full-page navigation so the app re-bootstraps
// unauthenticated (which redirects to the login page via the UnauthorizedInterceptor).
@Injectable({ providedIn: 'root' })
export class LogoutService {
  constructor(private http: HttpClient) {}

  async logout(): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post('/Identity/Account/Logout', null, { responseType: 'text' })
      );
    } finally {
      window.location.assign('/');
    }
  }
}
