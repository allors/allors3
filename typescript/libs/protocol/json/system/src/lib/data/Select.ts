import { Node } from "./Node";

export interface Select {
  /** AssociatoinType */
  a?: string;

  /** RoleType */
  r?: string;

  /** Next */
  n?: Select;

  /** Include */
  i: Node[];
}
