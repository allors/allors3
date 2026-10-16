import { ReadCache } from './read-cache';
import { ReadPull } from './read-pull';
import { ReadScope } from './read-scope';

export interface ReadRequest {
  /** Protocol revision. */
  v: number;

  /** Request ID, echoed by the response. */
  i: string;

  /** Expected scope; the server must independently establish its scope. */
  sc?: ReadScope | null;

  /** Operation context. */
  x?: string | null;

  /** Discovery never represents a completed read. */
  k: 'discover' | 'read';

  /** Declarative selections and dependencies. */
  p: ReadPull;

  /** Held cache candidates, absent for a cold read. */
  h?: ReadCache | null;
}
