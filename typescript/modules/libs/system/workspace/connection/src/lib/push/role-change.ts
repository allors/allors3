import { RoleType } from '@allors/system/workspace/meta';
import { IUnit } from '../types';

/**
 * The new value of a role, in the shape a record holds it: a unit, the id of a composite
 * role, or the ids of a composites role; null removes the role.
 */
export interface RoleChange {
  roleType: RoleType;

  value: IUnit | number | number[] | null | undefined;
}
