import { InvokeRequest, InvokeResponse, PullRequest, PullResponse, PushRequest, PushResponse, SecurityRequest, SecurityResponse, SyncRequest, SyncResponse } from '@allors/protocol/json/system';

export interface Client {
  baseUrl: string;
  userId: number;
  jwtToken: string;

  pull(pullRequest: PullRequest): Promise<PullResponse>;

  sync(syncRequest: SyncRequest): Promise<SyncResponse>;

  push(pushRequest: PushRequest): Promise<PushResponse>;

  invoke(invokeRequest: InvokeRequest): Promise<InvokeResponse>;

  security(securityRequest: SecurityRequest): Promise<SecurityResponse>;
}
