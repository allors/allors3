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
import { Subscribable } from './subscribable';

/**
 * A message the server sends on its own, over a transport that keeps a stream open. Server
 * push defines its content; until then no transport sends one.
 */
export class ServerMessage {}

/**
 * Carries the requests of a connection to the server and brings its responses back. The
 * requests and responses are the JSON protocol; a transport decides how they travel: over
 * HTTP with the application's own client, or in a test without a wire. The connection and its
 * transports are the only users of the protocol types.
 */
export interface ITransport {
  /**
   * The messages the server sends on its own, for a transport that keeps a stream open; null
   * for a request-response transport such as HTTP.
   */
  readonly serverMessages: Subscribable<ServerMessage> | null;

  pull(request: PullRequest): Promise<PullResponse>;

  sync(request: SyncRequest): Promise<SyncResponse>;

  push(request: PushRequest): Promise<PushResponse>;

  invoke(request: InvokeRequest): Promise<InvokeResponse>;

  access(request: AccessRequest): Promise<AccessResponse>;

  permission(request: PermissionRequest): Promise<PermissionResponse>;
}
