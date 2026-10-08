import { IRecord } from './record';

/**
 * A record the connection held has been replaced by a newer one.
 */
export class RecordChangedEvent {
  constructor(public readonly record: IRecord) {}

  get id(): number {
    return this.record.id;
  }
}
