import { IUnit } from '@allors/system/workspace/domain';
import { Extent } from './extent';
import { PredicateKind } from './predicate-kind';

export interface Predicate {
  /** kind */
  k: PredicateKind;

  /** AssociationType */
  a?: string;

  /** RoleType */
  r?: string;

  /** ObjectType */
  o?: string;

  /** Parameter */
  p?: string;

  /** Dependencies */
  d?: string[];

  /** Operand */
  op?: Predicate;

  /** Operands */
  ops?: Predicate[];

  /** Object */
  ob?: number;

  /** Objects */
  obs?: number[];

  /** Value */
  v?: IUnit;

  /** Values */
  vs?: IUnit[];

  /** Path Role Type */
  pa?: string;

  /** Path Role Type Tags */
  pas?: string[];

  /** Extent */
  e?: Extent;
}
