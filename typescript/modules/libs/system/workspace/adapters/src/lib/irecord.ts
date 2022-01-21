import { RoleType } from '@allors/system/workspace/meta';

export interface IRecord {
  version: number;

  getRole(roleType: RoleType): unknown;
}
