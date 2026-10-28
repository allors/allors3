import { Request } from '../request';

export interface SyncRequest extends Request {
  /** Objects */
  o: number[];

  /** Complete cached grants [id, version] */
  g?: number[][];

  /** Complete cached revocations [id, version] */
  r?: number[][];
}
