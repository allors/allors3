import { ReadInput } from './read-input';
import { ReadPermission } from './read-permission';
import { ReadRecord } from './read-record';
import { ReadScope } from './read-scope';
import { ReadSecurity } from './read-security';

export interface ReadSuccess {
  /** Echoed request ID. */
  i: string;

  /** Server-established read scope. */
  sc: ReadScope;

  k: 'complete';

  /** An outcome for each request selection. */
  l: ReadInput[];

  /** Named collections, including declared empty collections. */
  c: { [name: string]: number[] };

  /** Named objects, with null for declared absent objects. */
  o: { [name: string]: number | null };

  /** Complete requested record pool. */
  p: ReadRecord[];

  /** Security closure; pool provenance indexes refer to this table. */
  s: ReadSecurity[];

  /** Permission descriptors required by the security closure. */
  pm: ReadPermission[];
}
