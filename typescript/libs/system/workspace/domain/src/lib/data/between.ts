import { RoleType } from '@allors/system/workspace/meta';
import { IUnit } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Between extends ParameterizablePredicateBase {
  kind: 'Between';

  roleType: RoleType;

  values?: IUnit[];

  paths?: RoleType[];
}
