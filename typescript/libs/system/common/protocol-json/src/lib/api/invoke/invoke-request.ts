import { Request } from '../request';
import { Invocation } from './invocation';
import { InvokeOptions } from './invoke-options';

export interface InvokeRequest extends Request {
  /** List of Invocations */
  l: Invocation[];

  /** Options */
  o: InvokeOptions;
}
