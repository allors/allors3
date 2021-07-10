import { RoleType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';
import { UnitType } from '../runtime/Types';

export interface LessThan extends ParameterizablePredicateBase {
  kind: 'LessThan';
  roleType: RoleType;
  value?: UnitType;
  path?: RoleType;
}
