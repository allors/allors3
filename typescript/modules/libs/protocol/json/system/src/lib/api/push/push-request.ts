import { PushRequestNewObject } from './push-request-new-object';
import { PushRequestObject } from './push-request-object';

export interface PushRequest {
  /** NewObjects*/
  n?: PushRequestNewObject[];

  /** Objects */
  o?: PushRequestObject[];
}
