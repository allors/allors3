import { ObjectType, PropertyType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Instanceof extends ParameterizablePredicateBase {
  kind: 'Instanceof';
  propertyType: PropertyType;
  objectType?: ObjectType;
}
