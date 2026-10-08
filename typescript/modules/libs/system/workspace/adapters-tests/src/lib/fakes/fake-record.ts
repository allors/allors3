import { IRange, IRecord } from '@allors/system/workspace/connection';
import { Class, RoleType } from '@allors/system/workspace/meta';

/**
 * A record with an id and a version and nothing else, for the cache tests.
 */
export class FakeRecord implements IRecord {
  readonly grantIds: IRange<number> = undefined;

  readonly revocationIds: IRange<number> = undefined;

  constructor(
    public readonly cls: Class,
    public readonly id: number,
    public readonly version: number
  ) {}

  getRole(roleType: RoleType): unknown {
    return null;
  }

  isPermitted(permission: number): boolean {
    return false;
  }
}
