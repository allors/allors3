import { RoleType } from '@allors/workspace/meta/system';
import { UnitTypes } from '../runtime/Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Between extends ParameterizablePredicateBase {
  kind: 'Between';
  roleType: RoleType;

  values?: UnitTypes[];
  
  paths?: RoleType[];
}
