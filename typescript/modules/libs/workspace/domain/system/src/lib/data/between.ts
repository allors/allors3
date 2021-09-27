import { RoleType } from '@allors/workspace/meta/system';
import { IUnit } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Between extends ParameterizablePredicateBase {
  kind: 'Between';

  roleType: RoleType;

  values?: IUnit[];

  paths?: RoleType[];
}
