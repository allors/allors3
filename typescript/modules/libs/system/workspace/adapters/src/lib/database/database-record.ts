import { Class, RoleType } from '@allors/system/workspace/meta';
import { IRecord } from '../irecord';

export abstract class DatabaseRecord implements IRecord {
  abstract getRole(roleType: RoleType);

  abstract isPermitted(permission: number): boolean;

  constructor(
    public readonly cls: Class,
    public readonly id: number,
    public readonly version: number
  ) {}
}
