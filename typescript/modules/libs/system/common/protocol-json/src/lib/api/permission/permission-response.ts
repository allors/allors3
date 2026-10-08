import { Response } from '../response';
import { PermissionResponsePermission } from './permission-response-permission';

export interface PermissionResponse extends Response {
  /** Permissions */
  p: PermissionResponsePermission[];
}
