export interface ReadInput {
  /** Zero-based index into the request's selections. */
  i: number;

  /** Explicit selection target, when supplied. */
  o?: number | null;

  /** Unavailable does not distinguish missing from workspace-ineligible objects. */
  s: 'resolved' | 'unavailable';
}
