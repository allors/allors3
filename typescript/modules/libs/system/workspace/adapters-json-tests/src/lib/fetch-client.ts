import fetch from 'cross-fetch';
import { Agent } from 'http';
import {
  InvokeRequest,
  PullRequest,
  PullResponse,
  PushRequest,
  PushResponse,
  Response,
  SyncRequest,
  SyncResponse,
  AccessRequest,
  AccessResponse,
  PermissionRequest,
  PermissionResponse,
} from '@allors/system/common/protocol-json';
import { IDatabaseJsonClient } from '@allors/system/workspace/adapters-json';

// Test-only credential recognised by the Core test-harness server (see TestUserAuthenticationHandler):
// a request carrying this header is authenticated as that user without a password.
const TEST_USER_HEADER = 'X-Allors-TestUser';

interface UserInfoResponse {
  /** User id */
  u: string;

  /** User name */
  userName: string;
}

// Node 19+ defaults http(s) keep-alive to ON. node-fetch v2 (what cross-fetch uses) then
// reuses sockets that Kestrel closes between requests, which it surfaces as "Premature
// close" (fails every adapters-json test on Node 24 CI). Force a fresh, non-keep-alive
// connection per request. `agent` is a node-fetch option, not part of the standard fetch
// RequestInit, so the augmented init is passed untyped.
const keepAliveOffAgent = new Agent({ keepAlive: false });
const withAgent = (init: RequestInit = {}): RequestInit =>
  ({ ...init, agent: keepAliveOffAgent } as RequestInit);

export class FetchClient implements IDatabaseJsonClient {
  userId: number;
  userName: string;

  constructor(public baseUrl: string) {}

  async setup(population = 'full') {
    const url = `${this.baseUrl}Test/Setup?population=${population}`;
    await fetch(url, withAgent());
  }

  async login(login: string, password?: string): Promise<boolean> {
    // Bearer/JWT is retired; authenticate with the X-Allors-TestUser header and learn the user id
    // from the authenticated UserInfo endpoint (replacing the old token response's `u`). The
    // password argument is kept for call-site compatibility but is unused.
    this.userName = login;

    const response = await fetch(
      `${this.baseUrl}UserInfo`,
      withAgent({
        headers: {
          [TEST_USER_HEADER]: login,
        },
      })
    );

    if (response.ok) {
      const userInfo = (await response.json()) as UserInfoResponse;
      this.userId = Number(userInfo.u);
      return true;
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

  async access(accessRequest: AccessRequest): Promise<AccessResponse> {
    return await this.post('access', accessRequest);
  }

  async permission(
    permissionRequest: PermissionRequest
  ): Promise<PermissionResponse> {
    return await this.post('permission', permissionRequest);
  }

  async post<T>(relativeUrl: string, data: any): Promise<T> {
    const response = await fetch(
      `${this.baseUrl}${relativeUrl}`,
      withAgent({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          [TEST_USER_HEADER]: this.userName,
        },
        body: JSON.stringify(data),
      })
    );

    return await response.json();
  }
}
