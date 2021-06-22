import { PropertyType } from '@allors/workspace/meta/system';
import { Node } from './Node';

export interface Step {
  propertyType: PropertyType;

  include?: Node[];

  next?: Step;
}
