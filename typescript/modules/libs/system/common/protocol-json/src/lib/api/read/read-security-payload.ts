/** Full permission IDs or an exact zero-based request-security reuse index. */
export type ReadSecurityPayload =
  | { p: number[]; r?: null }
  | { p?: null; r: number };
