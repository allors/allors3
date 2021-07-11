import { RoleType } from '@allors/workspace/meta/system';
import { IUnit } from '../runtime/Types';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Between extends ParameterizablePredicateBase {
  kind: 'Between';
  roleType: RoleType;

  values?: IUnit[];

  paths?: RoleType[];
}
