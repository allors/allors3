import { ReadRecordHeader } from './read-record-header';
import { ReadSecurityHint } from './read-security-hint';

export interface ReadCache {
  /** Security provenance and separate body-possession claims. */
  s: ReadSecurityHint[];

  /** Held record bodies; provenance indexes refer to this request's security table. */
  r: ReadRecordHeader[];

  /** IDs of held immutable permission descriptors. */
  p: number[];
}
