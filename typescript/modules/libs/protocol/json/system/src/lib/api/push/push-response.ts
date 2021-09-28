import { PushResponseNewObject } from './push-response-new-object';
import { Response } from '../response';

export interface PushResponse extends Response {
  /** NewObjects */
  n: PushResponseNewObject[];
}
