import { PropertyType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { ParameterizablePredicate } from './ParameterizablePredicate';

export interface Contains extends ParameterizablePredicate {
  propertyType: PropertyType;
  object?: IObject;
}
