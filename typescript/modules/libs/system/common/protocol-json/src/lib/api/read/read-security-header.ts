export interface ReadSecurityHeader {
  /** Security body kind, part of its identity. */
  k: 'grant' | 'revocation';

  /** Security object ID. */
  i: number;

  /** Committed security object version. */
  v: number;
}
