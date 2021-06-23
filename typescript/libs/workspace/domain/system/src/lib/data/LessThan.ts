import { RoleType } from '../meta/RoleType';
import { ParameterizablePredicate } from './ParameterizablePredicate';
import { UnitTypes } from '../runtime/Types';

export interface LessThan extends ParameterizablePredicate {
  roleType: RoleType;
  value?: UnitTypes;
  path?: RoleType;
}
