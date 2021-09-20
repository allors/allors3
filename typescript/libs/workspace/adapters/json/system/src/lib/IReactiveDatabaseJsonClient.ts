import { InvokeRequest, InvokeResponse, PullRequest, PullResponse, PushRequest, PushResponse, SyncRequest, SyncResponse, AccessRequest, AccessResponse, PermissionRequest, PermissionResponse } from '@allors/protocol/json/system';
import { Observable } from 'rxjs';

export interface IReactiveDatabaseJsonClient {
  pull(pullRequest: PullRequest): Observable<PullResponse>;

  sync(syncRequest: SyncRequest): Observable<SyncResponse>;

  push(pushRequest: PushRequest): Observable<PushResponse>;

  invoke(invokeRequest: InvokeRequest): Observable<InvokeResponse>;

  access(accessRequest: AccessRequest): Observable<AccessResponse>;

  permission(permissionRequest: PermissionRequest): Observable<PermissionResponse>;
}
