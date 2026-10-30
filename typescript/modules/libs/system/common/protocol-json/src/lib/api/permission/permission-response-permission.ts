export interface PermissionResponsePermission {
  /** Id */
  i: number;

  /** Class Tag */
  c: string;

  /** Operand Type Tag, omitted for Create */
  t?: string;

  /** Operation */
  o: number;
}
