import { Composite } from './composite';
import { PropertyType } from './property-type';

export interface Dependency {
  objectType: Composite;
  propertyType: PropertyType;
}
