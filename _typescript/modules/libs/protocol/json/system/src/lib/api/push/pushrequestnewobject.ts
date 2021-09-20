import { PushRequestRole } from "./PushRequestRole";

export interface PushRequestNewObject {
  /** WorkspaceId */
  w: number;

  /** ObjectType Tag*/
  t: string;

  /** Roles */
  r?: PushRequestRole[];
}
