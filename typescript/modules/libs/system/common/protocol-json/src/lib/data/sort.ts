import { SortDirection } from './sort-direction';

export interface Sort {
  /** RoleType */
  r: string;

  /** Direction */
  d: SortDirection;
}
