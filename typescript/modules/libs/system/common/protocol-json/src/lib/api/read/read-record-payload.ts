import { ReadRole } from './read-role';

/** Full readable roles or an exact zero-based request-record reuse index. */
export type ReadRecordPayload =
  | { ro: ReadRole[]; r?: null }
  | { ro?: null; r: number };
