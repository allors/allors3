import {
  AccessRequest,
  AccessResponse,
  InvokeRequest,
  PermissionRequest,
  PermissionResponse,
  PullRequest,
  PullResponse,
  PushRequest,
  PushResponse,
  Response,
  SyncRequest,
  SyncResponse,
} from '@allors/system/common/protocol-json';
import { IDatabaseJsonClient } from '@allors/system/workspace/adapters-json';
import { HttpClient } from '@angular/common/http';

export class AppClient implements IDatabaseJsonClient {
  constructor(
    public httpClient: HttpClient,
    public baseUrl: string,
    public authUrl: string
  ) {}

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
    return this.httpClient
      .post<T>(`${this.baseUrl}${relativeUrl}`, data)
      .toPromise();
  }

  async setup(population = 'full') {
    await this.httpClient
      .get(`${this.baseUrl}Test/Setup?population=${population}`)
      .toPromise();
  }
}
