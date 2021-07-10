import { SyncResponseRole } from "./SyncResponseRole";

export interface SyncResponseObject {
  /** Id */
  i: number;

  /** Version */
  v: number;

  /** ObjectType */
  t: number;

  /** AccessControls (Range) */
  a: number[];

  /** DeniedPermissions (Range) */
  d: number[];

  /** Roles */
  r: SyncResponseRole[];
}
