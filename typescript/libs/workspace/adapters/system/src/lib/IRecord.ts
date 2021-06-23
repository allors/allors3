import { RoleType } from '@allors/workspace/domain/system';

export interface IRecord {
  version: number;

  getRole(roleType: RoleType): any;
}
