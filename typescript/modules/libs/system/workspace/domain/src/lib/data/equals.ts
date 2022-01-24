import { PropertyType, RoleType } from '@allors/system/workspace/meta';
import { IObject } from '../iobject';
import { IUnit } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType?: PropertyType;
  value?: IUnit;
  object?: IObject;
  path?: RoleType;
}
