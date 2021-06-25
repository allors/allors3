import { Extent } from './Extent';
import { Sort } from './Sort';

export interface Operator {
  operands?: Extent[];

  sorting?: Sort[];
}
