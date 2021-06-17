import { InvokeRequest, InvokeResponse, PullRequest, PullResponse, PushRequest, PushResponse, SecurityRequest, SecurityResponse, SyncRequest, SyncResponse } from '@allors/protocol/json/system';
import { Observable } from 'rxjs';

export interface Client {
  baseUrl: string;
  userId: number;
  jwtToken: string;

  pull(pullRequest: PullRequest): Observable<PullResponse>;

  sync(syncRequest: SyncRequest): Observable<SyncResponse>;

  push(pushRequest: PushRequest): Observable<PushResponse>;

  invoke(invokeRequest: InvokeRequest): Observable<InvokeResponse>;

  security(securityRequest: SecurityRequest): Observable<SecurityResponse>;
}
