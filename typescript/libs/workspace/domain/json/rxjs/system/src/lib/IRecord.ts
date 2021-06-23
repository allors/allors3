import { RoleType } from '@allors/workspace/system';

export interface IRecord {
  version: number;

  getRole(roleType: RoleType): any;
}
