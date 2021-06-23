import { PropertyType } from '../meta/PropertyType';
import { Node } from './Node';

export interface Step {
  propertyType: PropertyType;

  include?: Node[];

  next?: Step;
}
