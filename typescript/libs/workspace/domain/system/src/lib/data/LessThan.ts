import { RoleType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';
import { UnitTypes } from '../runtime/Types';

export interface LessThan extends ParameterizablePredicateBase {
  kind: 'LessThan';
  roleType: RoleType;
  value?: UnitTypes;
  path?: RoleType;
}
