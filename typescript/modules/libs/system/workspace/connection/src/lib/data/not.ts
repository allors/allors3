import { Predicate, PredicateBase } from './predicate';

export interface Not extends PredicateBase {
  kind: 'Not';
  operand?: Predicate;
}
