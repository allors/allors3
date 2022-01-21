import { ObjectType, PropertyType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Instanceof extends ParameterizablePredicateBase {
  kind: 'Instanceof';
  propertyType?: PropertyType;
  objectType?: ObjectType;
}
