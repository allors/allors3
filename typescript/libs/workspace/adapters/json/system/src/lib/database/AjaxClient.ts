import {
  AuthenticationTokenRequest,
  AuthenticationTokenResponse,
  InvokeRequest,
  PullRequest,
  PullResponse,
  PushRequest,
  PushResponse,
  Response,
  SecurityRequest,
  SecurityResponse,
  SyncRequest,
  SyncResponse,
} from '@allors/protocol/json/system';
import { Observable } from 'rxjs';
import { ajax, AjaxRequest } from 'rxjs/ajax';
import { map, tap } from 'rxjs/operators';
import { Client } from './Client';

export class AjaxClient implements Client {
  userId: number;
  jwtToken: string;

  constructor(public baseUrl: string, public authUrl: string) {}

  login(login: string, password?: string): Observable<boolean> {
    const tokenRequest: Partial<AuthenticationTokenRequest> = {
      l: login,
      p: password,
    };

    const ajaxRequest: AjaxRequest = {
      url: `${this.baseUrl}${this.authUrl}`,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: tokenRequest,
    };

    return ajax(ajaxRequest).pipe(
      map((ajaxResponse) => {
        const authenticationResponse = ajaxResponse.response as AuthenticationTokenResponse;
        return authenticationResponse;
      }),
      tap((authenticationResponse) => {
        if (authenticationResponse.a) {
          this.userId = authenticationResponse.u;
          this.jwtToken = authenticationResponse.t;
        }
      }),
      map((authenticationResponse) => authenticationResponse.a)
    );
  }

  pull(pullRequest: PullRequest): Observable<PullResponse> {
    return this.post('pull', pullRequest);
  }

  sync(syncRequest: SyncRequest): Observable<SyncResponse> {
    return this.post('sync', syncRequest);
  }

  push(pushRequest: PushRequest): Observable<PushResponse> {
    return this.post('sync', pushRequest);
  }
  invoke(invokeRequest: InvokeRequest): Observable<Response> {
    return this.post('sync', invokeRequest);
  }
  security(securityRequest: SecurityRequest): Observable<SecurityResponse> {
    return this.post('sync', securityRequest);
  }

  private post<T>(relativeUrl: string, body: any) {
    return ajax({
      url: `${this.baseUrl}${relativeUrl}`,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.jwtToken}`,
      },
      body,
    }).pipe(map((ajaxResponse) => ajaxResponse.response as T));
  }
}
