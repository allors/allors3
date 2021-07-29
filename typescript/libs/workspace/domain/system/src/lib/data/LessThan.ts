import { RoleType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';
import { IUnit } from '../runtime/Types';

export interface LessThan extends ParameterizablePredicateBase {
  kind: 'LessThan';
  roleType: RoleType;
  value?: IUnit;
  path?: RoleType;
}
