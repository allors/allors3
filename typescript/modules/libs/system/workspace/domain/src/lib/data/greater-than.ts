import { RoleType } from '@allors/system/workspace/meta';
import { IUnit } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface GreaterThan extends ParameterizablePredicateBase {
  kind: 'GreaterThan';
  roleType: RoleType;
  value?: IUnit;
  path?: RoleType;
}
