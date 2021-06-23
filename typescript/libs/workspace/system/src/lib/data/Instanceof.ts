import { ObjectType } from '../meta/ObjectType';
import { PropertyType } from '../meta/PropertyType';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Instanceof extends ParameterizablePredicate {
  propertyType: PropertyType;
  objectType?: ObjectType;
}
