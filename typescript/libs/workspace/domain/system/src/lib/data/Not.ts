import { Predicate, PredicateBase } from "./Predicate";

export interface Not extends PredicateBase {
  kind: 'Not',
  operand?: Predicate;
}
