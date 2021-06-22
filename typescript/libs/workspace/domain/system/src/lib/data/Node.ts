import { PropertyType } from '@allors/workspace/meta/system';
import { IVisitable } from './visitor/IVisitable';

export interface Node extends IVisitable {
  propertyType: PropertyType;
  nodes?: Node[];
}
