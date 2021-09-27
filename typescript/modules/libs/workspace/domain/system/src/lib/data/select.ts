import { PropertyType } from '@allors/workspace/meta/system';
import { Node } from './node';

export interface Select {
  propertyType: PropertyType;

  next?: Select;

  include?: Node[];
}
