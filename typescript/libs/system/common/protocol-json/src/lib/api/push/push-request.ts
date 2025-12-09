import { Request } from '../request';
import { PushRequestNewObject } from './push-request-new-object';
import { PushRequestObject } from './push-request-object';

export interface PushRequest extends Request {
  /** NewObjects*/
  n?: PushRequestNewObject[];

  /** Objects */
  o?: PushRequestObject[];
}
