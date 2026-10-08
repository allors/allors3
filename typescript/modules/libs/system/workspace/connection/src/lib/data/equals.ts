import { PropertyType, RoleType } from '@allors/system/workspace/meta';
import { IIdentifiable } from '../types';
import { IUnit } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Equals extends ParameterizablePredicateBase {
  kind: 'Equals';
  propertyType?: PropertyType;
  value?: IUnit;
  object?: IIdentifiable;
  objectId?: number;
  path?: RoleType;
}
