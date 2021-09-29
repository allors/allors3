import { PropertyType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';
import { Extent } from './extent';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface ContainedIn extends ParameterizablePredicateBase {
  kind: 'ContainedIn';
  propertyType: PropertyType;
  extent?: Extent;
  objects?: Array<IObject>;

  objectIds?: Array<number> | Array<string>;
}
