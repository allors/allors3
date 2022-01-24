import { RoleType } from '@allors/system/workspace/meta';
import { SortDirection } from './sort-direction';

export interface Sort {
  roleType: RoleType;
  sortDirection?: SortDirection;
}
