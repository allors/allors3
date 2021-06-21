import { IExtent } from './IExtent';
import { Sort } from './Sort';
import { IVisitable } from './visitor/IVisitable';

export interface Operator extends IVisitable {
  operands?: IExtent[];

  sorting?: Sort[];
}
