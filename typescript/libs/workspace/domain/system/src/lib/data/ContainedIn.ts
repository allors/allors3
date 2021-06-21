import { PropertyType } from '@allors/workspace/meta/system';
import { ParameterizablePredicate } from './ParameterizablePredicate';
import { IExtent } from './IExtent';
import { IObject } from '../IObject';

export interface ContainedIn extends ParameterizablePredicate {
  propertyType: PropertyType;
  extent?: IExtent;
  objects?: Array<IObject>;
}
