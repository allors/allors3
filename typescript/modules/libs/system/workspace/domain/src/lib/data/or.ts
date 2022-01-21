import { Predicate, PredicateBase } from './predicate';

export interface Or extends PredicateBase {
  kind: 'Or';
  operands: Predicate[];
}
