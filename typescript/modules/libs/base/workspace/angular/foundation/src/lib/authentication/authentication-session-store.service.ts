import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { map, catchError } from 'rxjs/operators';
import { throwError, Observable } from 'rxjs';

import { UserId } from '../state/user-id';

import { AuthenticationConfig } from './authentication-config';
import { AuthenticationTokenResponse } from './authentication-token-response';
import { AuthenticationTokenRequest } from './authentication-token-request';
import { AuthenticationService } from './authentication.service';

@Injectable()
export class AuthenticationSessionStoreService extends AuthenticationService {
  private tokenName = 'ALLORS_JWT';

  public get token(): string | null {
    return sessionStorage.getItem(this.tokenName);
  }

  public set token(value: string | null) {
    if (value == null) {
      sessionStorage.removeItem(this.tokenName);
    } else {
      sessionStorage.setItem(this.tokenName, value);
    }
  }

  constructor(
    private http: HttpClient,
    private authenticationConfig: AuthenticationConfig,
    private userIdState: UserId,
    private router: Router
  ) {
    super();
  }

  login$(
    userName: string,
    password: string
  ): Observable<AuthenticationTokenResponse> {
    const url = this.authenticationConfig.url;
    const request: AuthenticationTokenRequest = { l: userName, p: password };

    return this.http.post<AuthenticationTokenResponse>(url, request).pipe(
      map((result: AuthenticationTokenResponse) => {
        if (result.a) {
          this.token = result.t;
          this.userIdState.value = result.u;
        }

        return result;
      }),
      catchError((error: any) => {
        const errMsg = error.message
          ? error.message
          : error.status
          ? `${error.status} - ${error.statusText}`
          : 'Server error';
        return throwError(errMsg);
      })
    );
  }

  logout() {
    this.token = null;
    this.router.navigate(['/login']);
  }
}
