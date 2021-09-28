import { Select } from './select';
import { Node } from './node';

export interface Result {
  /** SelectRef */
  r?: string;

  /** Select */
  s?: Select;

  /** Include */
  i: Node[];

  /** Name */
  n?: string;

  /** Skip */
  k?: number;

  /** Take */
  t?: number;
}
