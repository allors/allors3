import { Step } from './Step';
import { IVisitable } from './visitor/IVisitable';

export interface Select extends IVisitable {
  step?: Step;

  include?: Node[];
}
