import { RoleType } from '@allors/workspace/meta/system';
import { UnitType } from '../runtime/Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Between extends ParameterizablePredicateBase {
  kind: 'Between';
  roleType: RoleType;

  values?: UnitType[];

  paths?: RoleType[];
}
