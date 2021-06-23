import { SortDirection } from "@allors/workspace/system";

export interface Sort {
  /** RoleType */
  r: number;

  /** Direction */
  d: SortDirection;
}
