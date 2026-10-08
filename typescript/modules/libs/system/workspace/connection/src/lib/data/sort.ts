import { RoleType } from '@allors/system/workspace/meta';
import { SortDirection } from '@allors/system/common/protocol-json';

export interface Sort {
  roleType: RoleType;
  sortDirection?: SortDirection;
}
