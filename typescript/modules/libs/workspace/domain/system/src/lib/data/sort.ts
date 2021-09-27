import { RoleType } from '@allors/workspace/meta/system';
import { SortDirection } from './sort-direction';

export interface Sort {
  roleType: RoleType;
  sortDirection?: SortDirection;
}
