import { Request } from '../request';
import { Invocation } from './invocation';

export interface InvokeRequest extends Request {
  /** List of Invocations */
  l: Invocation[];

  /** Options */
}
