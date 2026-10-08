import { Response } from '../response';
import { SyncResponseObject } from './sync-response-object';

export interface SyncResponse extends Response {
  /** Objects */
  o: SyncResponseObject[];
}
