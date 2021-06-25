import { Predicate, PredicateBase } from "./Predicate";

export interface Not extends PredicateBase {
  operand?: Predicate;
}
