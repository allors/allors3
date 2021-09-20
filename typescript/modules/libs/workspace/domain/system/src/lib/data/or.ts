import { Predicate, PredicateBase } from './Predicate';

export interface Or extends PredicateBase {
  kind: 'Or';
  operands: Predicate[];
}
