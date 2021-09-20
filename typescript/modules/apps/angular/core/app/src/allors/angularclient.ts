import { InvokeRequest, PullRequest, PullResponse, PushRequest, PushResponse, Response, SecurityRequest, SecurityResponse, SyncRequest, SyncResponse } from '@allors/protocol/json/system';
import { IReactiveDatabaseJsonClient } from '@allors/workspace/adapters/json/system';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface AuthenticationTokenRequest {
  /** login */
  l: string;

  /** password */
  p: string;
}

interface AuthenticationTokenResponse {
  /** Authenticated */
  a: boolean;

  /** User id */
  u: number;

  /** Token */
  t: string;
}

export class AngularClient implements IReactiveDatabaseJsonClient {
  userId: number;
  jwtToken: string;
  httpHeaders: HttpHeaders;

  constructor(public httpClient: HttpClient, public baseUrl: string, public authUrl: string) {}

  async setup(population = 'full') {
    await this.httpClient.get(`${this.baseUrl}Test/Setup?population=${population}`).toPromise();
  }

  async login(login: string, password?: string): Promise<boolean> {
    const tokenRequest: Partial<AuthenticationTokenRequest> = {
      l: login,
      p: password,
    };

    const response = await this.httpClient
      .post(`${this.baseUrl}${this.authUrl}`, tokenRequest, {
        headers: new HttpHeaders({
          'Content-Type': 'application/json',
        }),
      })
      .toPromise();

    const tokenResponse = response as AuthenticationTokenResponse;

    if (tokenResponse.a) {
      this.userId = tokenResponse.u;
      this.jwtToken = tokenResponse.t;
      this.httpHeaders = new HttpHeaders({
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.jwtToken}`,
      });

      return true;
    }

    this.userId = null;
    this.jwtToken = null;
    this.httpHeaders = null;
    return false;
  }

  pull(pullRequest: PullRequest): Observable<PullResponse> {
    return this.post<PullResponse>('pull', pullRequest);
  }

  sync(syncRequest: SyncRequest): Observable<SyncResponse> {
    return this.post<SyncResponse>('sync', syncRequest);
  }

  push(pushRequest: PushRequest): Observable<PushResponse> {
    return this.post<PushResponse>('push', pushRequest);
  }

  invoke(invokeRequest: InvokeRequest): Observable<Response> {
    return this.post<Response>('invoke', invokeRequest);
  }

  security(securityRequest: SecurityRequest): Observable<SecurityResponse> {
    return this.post<SecurityResponse>('security', securityRequest);
  }

  post<T>(relativeUrl: string, data: any): Observable<T> {
    return this.httpClient.post<T>(`${this.baseUrl}${relativeUrl}`, data, {
      headers: this.httpHeaders,
    });
  }
}
