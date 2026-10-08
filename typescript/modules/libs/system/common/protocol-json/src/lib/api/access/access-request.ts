import { Request } from '../request';

export interface AccessRequest extends Request {
  /** Grants */
  g?: number[];

  /** Revocations */
  r?: number[];
}
