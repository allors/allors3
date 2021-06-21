import { PropertyType } from '@allors/workspace/meta/system';
import { ParameterizablePredicate } from './ParameterizablePredicate';
import { IObject } from '../IObject';

export interface Contains extends ParameterizablePredicate {
  propertyType: PropertyType;
  object?: IObject;
}
