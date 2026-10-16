import { ReadRecordHeader } from './read-record-header';
import { ReadScope } from './read-scope';
import { ReadSecurityHeader } from './read-security-header';

export interface DiscoverySuccess {
  /** Echoed request ID. */
  i: string;

  /** Server-established read scope. */
  sc: ReadScope;

  k: 'discovery';

  /** Discovered records without role payloads. */
  p: ReadRecordHeader[];

  /** Security provenance referenced by this discovery pool. */
  s: ReadSecurityHeader[];
}
