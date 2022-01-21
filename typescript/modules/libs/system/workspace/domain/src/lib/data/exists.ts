import { PropertyType } from '@allors/workspace/meta/system';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Exists extends ParameterizablePredicateBase {
  kind: 'Exists';

  propertyType: PropertyType;
}
