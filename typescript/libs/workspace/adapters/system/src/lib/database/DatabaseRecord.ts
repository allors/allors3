import { Class, RoleType } from '@allors/workspace/meta/system';
import { IRecord } from '../IRecord';

export abstract class DatabaseRecord implements IRecord {
  abstract getRole(roleType: RoleType);

  abstract isPermitted(permission: number): boolean;

  constructor(public readonly cls: Class, public readonly id: number, public readonly version: number) {}
}
