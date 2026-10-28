import { AccessResponseGrant } from '../access/access-response-grant';
import { AccessResponseRevocation } from '../access/access-response-revocation';
import { PermissionResponsePermission } from '../permission/permission-response-permission';
import { Response } from '../response';
import { SyncResponseObject } from './sync-response-object';
import { SyncResponseResult } from './sync-response-result';

export interface SyncResponse extends Response {
  result: SyncResponseResult;
  o: SyncResponseObject[];
  g: AccessResponseGrant[];
  r: AccessResponseRevocation[];
  p: PermissionResponsePermission[];
}
