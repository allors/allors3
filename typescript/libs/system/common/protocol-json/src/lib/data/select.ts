import { Node } from './node';

export interface Select {
  /** AssociatoinType */
  a?: string;

  /** RoleType */
  r?: string;

  /** OfType */
  o?: string;

  /** Next */
  n?: Select;

  /** Include */
  i: Node[];
}
