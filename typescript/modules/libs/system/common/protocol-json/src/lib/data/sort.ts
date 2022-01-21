import { SortDirection } from '@allors/system/workspace/domain';

export interface Sort {
  /** RoleType */
  r: string;

  /** Direction */
  d: SortDirection;
}
