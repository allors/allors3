export interface ReadScope {
  /** Population incarnation. */
  p: string;

  /** Authenticated user ID, established by the server. */
  u: number;

  /** Workspace name. */
  w: string;

  /** Schema and representation revision. */
  r: string;

  /** Selection and projection context. */
  x?: string | null;
}
