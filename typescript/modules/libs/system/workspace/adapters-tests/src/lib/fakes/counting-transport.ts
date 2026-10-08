import {
  AccessRequest,
  AccessResponse,
  InvokeRequest,
  InvokeResponse,
  PermissionRequest,
  PermissionResponse,
  PullRequest,
  PullResponse,
  PushRequest,
  PushResponse,
  SyncRequest,
  SyncResponse,
} from '@allors/system/common/protocol-json';
import { ITransport } from '@allors/system/workspace/connection';

/**
 * A transport around another that counts the calls and keeps the last request of each
 * kind, so that a test can say what a pull did and did not ask the server.
 */
export class CountingTransport implements ITransport {
  pullCount = 0;

  syncCount = 0;

  pushCount = 0;

  invokeCount = 0;

  accessCount = 0;

  permissionCount = 0;

  lastSyncRequest: SyncRequest;

  lastAccessRequest: AccessRequest;

  lastPermissionRequest: PermissionRequest;

  constructor(private readonly inner: ITransport) {}

  get serverMessages() {
    return this.inner.serverMessages;
  }

  pull(request: PullRequest): Promise<PullResponse> {
    this.pullCount++;
    return this.inner.pull(request);
  }

  sync(request: SyncRequest): Promise<SyncResponse> {
    this.syncCount++;
    this.lastSyncRequest = request;
    return this.inner.sync(request);
  }

  push(request: PushRequest): Promise<PushResponse> {
    this.pushCount++;
    return this.inner.push(request);
  }

  invoke(request: InvokeRequest): Promise<InvokeResponse> {
    this.invokeCount++;
    return this.inner.invoke(request);
  }

  access(request: AccessRequest): Promise<AccessResponse> {
    this.accessCount++;
    this.lastAccessRequest = request;
    return this.inner.access(request);
  }

  permission(request: PermissionRequest): Promise<PermissionResponse> {
    this.permissionCount++;
    this.lastPermissionRequest = request;
    return this.inner.permission(request);
  }
}
