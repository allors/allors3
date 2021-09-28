import { PushRequestRole } from './push-request-role';

export interface PushRequestNewObject {
  /** WorkspaceId */
  w: number;

  /** ObjectType Tag*/
  t: string;

  /** Roles */
  r?: PushRequestRole[];
}
