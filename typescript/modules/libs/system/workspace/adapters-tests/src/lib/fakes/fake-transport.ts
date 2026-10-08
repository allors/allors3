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
import { M } from '@allors/default/workspace/meta';
import { FakeServer } from './fake-server';

/**
 * The transport to a FakeServer: no wire, the requests and responses pass as objects.
 */
export class FakeTransport implements ITransport {
  readonly serverMessages = null;

  readonly server: FakeServer;

  constructor(m: M) {
    this.server = new FakeServer(m);
  }

  pull(request: PullRequest): Promise<PullResponse> {
    return Promise.resolve(this.server.pull(request));
  }

  sync(request: SyncRequest): Promise<SyncResponse> {
    return Promise.resolve(this.server.sync(request));
  }

  push(request: PushRequest): Promise<PushResponse> {
    return Promise.resolve(this.server.push(request));
  }

  invoke(request: InvokeRequest): Promise<InvokeResponse> {
    return Promise.resolve(this.server.invoke(request));
  }

  access(request: AccessRequest): Promise<AccessResponse> {
    return Promise.resolve(this.server.access(request));
  }

  permission(request: PermissionRequest): Promise<PermissionResponse> {
    return Promise.resolve(this.server.permission(request));
  }
}
