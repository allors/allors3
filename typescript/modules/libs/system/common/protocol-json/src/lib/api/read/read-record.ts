import { ReadRecordHeader } from './read-record-header';
import { ReadRecordPayload } from './read-record-payload';

export interface ReadRecord extends ReadRecordHeader {
  /** Full roles or exact request-record reuse; header provenance remains current. */
  p: ReadRecordPayload;
}
