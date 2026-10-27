import { AccessResponseGrant } from '../access/access-response-grant';
import { AccessResponseRevocation } from '../access/access-response-revocation';
import { PermissionResponsePermission } from '../permission/permission-response-permission';
import { SyncResponseObject } from './sync-response-object';

export interface SyncResponse {
  /** Objects */
  o: SyncResponseObject[];

  /** Grants used by the objects */
  g: AccessResponseGrant[];

  /** Revocations used by the objects */
  r: AccessResponseRevocation[];

  /** Permission definitions for the included access entries */
  p: PermissionResponsePermission[];
}
