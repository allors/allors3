import { Extent } from './extent';
import { Sort } from './sort';
import { Except } from './except';
import { Intersect } from './intersect';
import { Union } from './union';

export type Operator = Except | Intersect | Union;

export type OperatorKind = Operator['kind'];

export interface OperatorBase {
  operands?: Extent[];

  sorting?: Sort[];
}
