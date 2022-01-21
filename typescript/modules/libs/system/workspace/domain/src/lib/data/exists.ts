import { PropertyType } from '@allors/system/workspace/meta';
import { ParameterizablePredicateBase } from './parameterizable-predicate';

export interface Exists extends ParameterizablePredicateBase {
  kind: 'Exists';

  propertyType: PropertyType;
}
