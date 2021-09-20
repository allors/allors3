import { PermissionResponsePermission } from "./PermissionResponsePermission";

export interface PermissionResponse {
  /** AccessControls */
  p: PermissionResponsePermission[];
}
