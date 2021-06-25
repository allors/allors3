import { Predicate, PredicateBase } from './Predicate';

export interface And extends PredicateBase {
  kind: 'And';
  operands: Predicate[];
}
