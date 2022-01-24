import { PropertyType } from '@allors/system/workspace/meta';
import { Node } from './node';

export interface Select {
  propertyType: PropertyType;

  next?: Select;

  include?: Node[];
}
