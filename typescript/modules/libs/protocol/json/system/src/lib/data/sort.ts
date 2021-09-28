import { SortDirection } from '@allors/workspace/domain/system';

export interface Sort {
  /** RoleType */
  r: string;

  /** Direction */
  d: SortDirection;
}
