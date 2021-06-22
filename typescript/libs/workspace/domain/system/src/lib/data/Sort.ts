import { RoleType } from '@allors/workspace/meta/system';
import { SortDirection } from './SortDirection';
import { IVisitable } from './visitor/IVisitable';

export interface Sort extends IVisitable {
  roleType: RoleType;
  sortDirection: SortDirection;
}
