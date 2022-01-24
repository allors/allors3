import { RoleType } from '@allors/system/workspace/meta';
import { ParameterizablePredicateBase } from './parameterizable-predicate';
import { IUnit } from '../types';

export interface LessThan extends ParameterizablePredicateBase {
  kind: 'LessThan';
  roleType: RoleType;
  value?: IUnit;
  path?: RoleType;
}
