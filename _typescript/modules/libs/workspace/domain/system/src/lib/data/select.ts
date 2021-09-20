import { PropertyType } from '@allors/workspace/meta/system';
import { Node } from './Node';

export interface Select {
  propertyType: PropertyType;

  next?: Select;

  include?: Node[];
}
