import { Step } from './Step';
import { IVisitable } from './visitor/IVisitable';
import { Node } from './Node';

export interface Select extends IVisitable {
  step?: Step;

  include?: Node[];
}
