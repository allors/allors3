import { PropertyType, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';
import { IUnit } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType?: PropertyType;
  value?: IUnit;
  object?: IObject;
  objectId?: number;
  path?: RoleType;
}
