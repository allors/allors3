import { RoleType } from '@allors/workspace/meta/system';
import { SortDirection } from './SortDirection';

export interface Sort {
  roleType: RoleType;
  sortDirection: SortDirection;
}
