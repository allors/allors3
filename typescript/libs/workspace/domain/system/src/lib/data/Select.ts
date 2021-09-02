import { Node } from './Node';
import { PropertyType } from '@allors/workspace/meta/system';

export interface Select {
  propertyType?: PropertyType;

  next?: Select;

  include?: Node[];
}
