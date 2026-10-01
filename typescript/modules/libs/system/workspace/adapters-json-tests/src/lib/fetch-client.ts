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
// a request carrying this header is authenticated, without a password, as the user with that UniqueId.
const TEST_USER_HEADER = 'X-Allors-TestUser';

// The UniqueIds of the users of the Core test population (Users in the Core test domain); the tests
// keep naming the users by these aliases.
const TEST_USER_IDS: Record<string, string> = {
  administrator: '880AFBDD-E1D3-4382-A54B-1E008DA58CB7',
  'jane@example.com': '52396749-5CFF-4D1D-889F-DFD12753945F',
  agent: 'E0310978-B016-40D6-926B-1155A4B9BC82',
  noacl: '5CF05A59-C6E2-44DD-87F8-ADB6A670ED0A',
  noperm: '171125FF-E79A-447C-86D8-018BB88C09A7',
};

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
  testUserId: string;

  constructor(public baseUrl: string) {}

  async setup(population = 'full') {
    const url = `${this.baseUrl}Test/Setup?population=${population}`;
    await fetch(url, withAgent());
  }

  async login(login: string, password?: string): Promise<boolean> {
    // Authenticate with the X-Allors-TestUser header, which carries the UniqueId of the user, and
    // learn the user id from the authenticated UserInfo endpoint. The password argument is kept for
    // call-site compatibility but is unused.
    const testUserId = TEST_USER_IDS[login];
    if (testUserId === undefined) {
      throw new Error(
        `'${login}' is not a user of the test population. Sign in as one of: ${Object.keys(
          TEST_USER_IDS
        ).join(', ')}.`
      );
    }

    this.userName = login;
    this.testUserId = testUserId;

    const response = await fetch(
      `${this.baseUrl}UserInfo`,
      withAgent({
        headers: {
          [TEST_USER_HEADER]: testUserId,
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
          [TEST_USER_HEADER]: this.testUserId,
        },
        body: JSON.stringify(data),
      })
    );

    return await response.json();
  }
}
