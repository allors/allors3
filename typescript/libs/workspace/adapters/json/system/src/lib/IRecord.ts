import { RoleType } from '@allors/workspace/meta/system';

export interface IRecord {

  version : number;

  getRole(roleType: RoleType): any;
}
