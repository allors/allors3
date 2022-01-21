import { PropertyType } from '@allors/system/workspace/meta';
import { IObject } from '../iobject';
import { Extent } from './extent';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface ContainedIn extends ParameterizablePredicateBase {
  kind: 'ContainedIn';
  propertyType: PropertyType;
  extent?: Extent;
  objects?: Array<IObject>;
  objectIds?: Array<number>;
}
