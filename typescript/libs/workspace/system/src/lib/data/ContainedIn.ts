import { PropertyType } from '../meta/PropertyType';
import { IObject } from '../runtime/IObject';
import { IExtent } from './IExtent';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface ContainedIn extends ParameterizablePredicate {
  propertyType: PropertyType;
  extent?: IExtent;
  objects?: Array<IObject>;
}
