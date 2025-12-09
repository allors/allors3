import { PermissionResponsePermission } from './permission-response-permission';

export interface PermissionResponse {
  /** AccessControls */
  p: PermissionResponsePermission[];
}
