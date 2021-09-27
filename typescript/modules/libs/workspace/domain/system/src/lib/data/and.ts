import { Predicate, PredicateBase } from './predicate';

export interface And extends PredicateBase {
  readonly kind: 'And';
  operands: Predicate[];
}
