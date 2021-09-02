import { Node } from "./Node";

export interface Select {
  a?: number;

  /** RoleType */
  r?: number;

  /** Next */
  n?: Select;

  /** Include */
  i: Node[];
}
