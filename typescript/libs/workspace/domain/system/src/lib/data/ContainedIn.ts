import { PropertyType } from '@allors/workspace/meta/system';
import { IObject } from '../runtime/IObject';
import { Extent } from './Extent';
import { ParameterizablePredicateBase } from './ParameterizablePredicate';

export interface ContainedIn extends ParameterizablePredicateBase {
  kind: 'ContainedIn';
  propertyType: PropertyType;
  extent?: Extent;
  objects?: Array<IObject>;
}
