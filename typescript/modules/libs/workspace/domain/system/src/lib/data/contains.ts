import { PropertyType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Contains extends ParameterizablePredicateBase {
  kind: 'Contains';
  propertyType: PropertyType;
  object?: IObject;
  objectId?: number;
}
