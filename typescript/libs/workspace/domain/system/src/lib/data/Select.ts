import { Step } from './Step';
import { Node } from './Node';

export interface Select {
  step?: Step;

  include?: Node[];
}
