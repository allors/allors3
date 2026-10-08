import { PropertyType } from '@allors/system/workspace/meta';
import { IIdentifiable } from '../types';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Contains extends ParameterizablePredicateBase {
  kind: 'Contains';
  propertyType: PropertyType;
  object?: IIdentifiable;
  objectId?: number;
}
