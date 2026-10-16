import { ReadError } from './read-error';
import { ReadScope } from './read-scope';

export interface ReadFailure {
  /** Echoed request ID. */
  i: string;

  /** Server-established scope, when available. */
  sc?: ReadScope | null;

  k: 'error';

  /** Errors expressed without Session objects. */
  e: ReadError[];
}
