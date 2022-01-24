import { ExtentKind } from './extent-kind';
import { Predicate } from './predicate';
import { Sort } from './sort';

export interface Extent {
  /** Kind */
  k: ExtentKind;

  /** Operands */
  o?: Extent[];

  /** ObjectType */
  t?: string;

  /** Predicate */
  p?: Predicate;

  /** Sorting */
  s?: Sort[];
}
