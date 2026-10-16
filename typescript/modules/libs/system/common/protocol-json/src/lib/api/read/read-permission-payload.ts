/**
 * Full immutable descriptor or an exact zero-based request-permission reuse index.
 * Operation codes retain Create=1, Read=2, Write=4 and Execute=8.
 * Read/write operands are relation tags; execute operands are method tags.
 */
export type ReadPermissionPayload =
  | { c: string; o: 1; t?: null; r?: null }
  | { c: string; o: 2 | 4 | 8; t: string; r?: null }
  | { c?: null; o?: null; t?: null; r: number };
