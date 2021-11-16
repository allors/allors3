import { Request } from '../request';

export interface SyncRequest extends Request {
  /** Objects */
  o: number[];
}
