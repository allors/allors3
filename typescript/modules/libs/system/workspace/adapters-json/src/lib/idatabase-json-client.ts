import {
  InvokeRequest,
  InvokeResponse,
  PullRequest,
  PullResponse,
  PushRequest,
  PushResponse,
  SyncRequest,
  SyncResponse,
  AccessRequest,
  AccessResponse,
  PermissionRequest,
  PermissionResponse,
} from '@allors/system/common/protocol-json';

export interface IDatabaseJsonClient {
  pull(pullRequest: PullRequest): Promise<PullResponse>;

  sync(syncRequest: SyncRequest): Promise<SyncResponse>;

  push(pushRequest: PushRequest): Promise<PushResponse>;

  invoke(invokeRequest: InvokeRequest): Promise<InvokeResponse>;

  access(accessRequest: AccessRequest): Promise<AccessResponse>;

  permission(permissionRequest: PermissionRequest): Promise<PermissionResponse>;
}
