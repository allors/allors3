import { PropertyType } from '@allors/system/workspace/meta';
import { IIdentifiable } from '../types';
import { Extent } from './extent';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface ContainedIn extends ParameterizablePredicateBase {
  kind: 'ContainedIn';
  propertyType: PropertyType;
  extent?: Extent;
  objects?: Array<IIdentifiable>;
  objectIds?: Array<number>;
}
