import { ObjectType, PropertyType } from '@allors/system/workspace/meta';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Instanceof extends ParameterizablePredicateBase {
  kind: 'Instanceof';
  propertyType?: PropertyType;
  objectType?: ObjectType;
}
