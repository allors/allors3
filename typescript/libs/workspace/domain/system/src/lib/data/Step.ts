import { PropertyType } from '@allors/workspace/meta/system';
import { IVisitable } from './visitor/IVisitable';
import { Node } from './Node';

export interface Step extends IVisitable {
  propertyType: PropertyType;

  include?: Node[];

  next?: Step;
}
