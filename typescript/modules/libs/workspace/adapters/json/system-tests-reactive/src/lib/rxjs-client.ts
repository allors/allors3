import fetch from 'cross-fetch';
import { AccessRequest, AccessResponse, PermissionRequest, PermissionResponse, InvokeRequest, InvokeResponse, PullRequest, PullResponse, PushRequest, PushResponse, Response, SyncRequest, SyncResponse } from '@allors/protocol/json/system';
import { IReactiveDatabaseJsonClient } from '@allors/workspace/adapters/json/system';
import { from, Observable } from 'rxjs';

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

export class RxjsClient implements IReactiveDatabaseJsonClient {
  userId: number;
  jwtToken: string;

  constructor(public baseUrl: string, public authUrl: string) {}

  async setup(population = 'full') {
    await fetch(`${this.baseUrl}Test/Setup?population=${population}`);
  }

  async login(login: string, password?: string): Promise<boolean> {
    const tokenRequest: Partial<AuthenticationTokenRequest> = {
      l: login,
      p: password,
    };

    const response = await fetch(`${this.baseUrl}${this.authUrl}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(tokenRequest),
    });

    if (response.ok) {
      const tokenResponse = (await response.json()) as AuthenticationTokenResponse;

      if (tokenResponse.a) {
        this.userId = tokenResponse.u;
        this.jwtToken = tokenResponse.t;
        return true;
      }
    }

    return false;
  }

  pull(pullRequest: PullRequest): Observable<PullResponse> {
    return from(this.post<PullResponse>('pull', pullRequest));
  }

  sync(syncRequest: SyncRequest): Observable<SyncResponse> {
    return from(this.post<SyncResponse>('sync', syncRequest));
  }

  push(pushRequest: PushRequest): Observable<PushResponse> {
    return from(this.post<PushResponse>('push', pushRequest));
  }

  invoke(invokeRequest: InvokeRequest): Observable<InvokeResponse> {
    return from(this.post<Response>('invoke', invokeRequest));
  }

  access(accessRequest: AccessRequest): Observable<AccessResponse> {
    return from(this.post<AccessResponse>('access', accessRequest));
  }

  permission(permissionRequest: PermissionRequest): Observable<PermissionResponse> {
    return from(this.post<PermissionResponse>('permission', permissionRequest));
  }

  post<T>(relativeUrl: string, data: any): Promise<T> {
    const response = fetch(`${this.baseUrl}${relativeUrl}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.jwtToken}`,
      },
      body: JSON.stringify(data),
    });

    return response.then((v) => v.json());
  }
}
