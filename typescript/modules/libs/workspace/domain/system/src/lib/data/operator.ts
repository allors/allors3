import { Extent } from './Extent';
import { Sort } from './Sort';
import { Except } from './Except';
import { Intersect } from './Intersect';
import { Union } from './Union';

export type Operator = Except | Intersect | Union;

export type OperatorKind = Operator['kind'];

export interface OperatorBase {
  operands?: Extent[];

  sorting?: Sort[];
}
