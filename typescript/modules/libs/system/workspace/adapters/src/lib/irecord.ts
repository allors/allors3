import { RoleType } from '@allors/system/workspace/meta';

export interface IRecord {
  version: number;

  /** For a merge, a record can return a unit with its full stored precision. */
  getRole(roleType: RoleType, forComparison?: boolean): unknown;
}
