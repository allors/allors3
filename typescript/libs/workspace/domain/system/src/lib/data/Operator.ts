import { IExtent } from './IExtent';
import { Sort } from './Sort';

export interface Operator {
  operands?: IExtent[];

  sorting?: Sort[];
}
