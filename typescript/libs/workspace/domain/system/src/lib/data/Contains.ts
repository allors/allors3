import { PropertyType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface Contains extends ParameterizablePredicateBase {
  kind: 'Contains';
  propertyType: PropertyType;
  object?: IObject;
}
