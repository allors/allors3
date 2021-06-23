import { RoleType } from '../meta/RoleType';
import { SortDirection } from './SortDirection';

export interface Sort {
  roleType: RoleType;
  sortDirection: SortDirection;
}
