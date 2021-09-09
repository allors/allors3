import { Predicate, PredicateBase } from './Predicate';

export interface And extends PredicateBase {
  readonly kind: 'And';
  operands: Predicate[];
}
