export interface ReadError {
  /** Machine-readable error code. */
  c: string;

  /** Actionable error message. */
  m: string;

  /** Related database object IDs, when available. */
  o?: number[] | null;

  /** Related zero-based selection index, when available. */
  i?: number | null;
}
