import { RoleType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './parameterizable-predicate';
import { IUnit } from '../types';

export interface LessThan extends ParameterizablePredicateBase {
  kind: 'LessThan';
  roleType: RoleType;
  value?: IUnit;
  path?: RoleType;
}
