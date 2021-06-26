import fetch from 'cross-fetch';
import { InvokeRequest, PullRequest, PullResponse, PushRequest, PushResponse, Response, SecurityRequest, SecurityResponse, SyncRequest, SyncResponse } from '@allors/protocol/json/system';
import { Client } from '@allors/workspace/adapters/json/system';

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

export class FetchClient implements Client {
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

  async pull(pullRequest: PullRequest): Promise<PullResponse> {
    return await this.post('pull', pullRequest);
  }

  async sync(syncRequest: SyncRequest): Promise<SyncResponse> {
    return await this.post('sync', syncRequest);
  }

  async push(pushRequest: PushRequest): Promise<PushResponse> {
    return await this.post('push', pushRequest);
  }

  async invoke(invokeRequest: InvokeRequest): Promise<Response> {
    return await this.post('invoke', invokeRequest);
  }
  
  async security(securityRequest: SecurityRequest): Promise<SecurityResponse> {
    return await this.post('security', securityRequest);
  }

  async post<T>(relativeUrl: string, data: any): Promise<T> {
    const response = await fetch(`${this.baseUrl}${relativeUrl}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${this.jwtToken}`,
      },
      body: JSON.stringify(data),
    });

    return await response.json();
  }
}
