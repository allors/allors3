import { Invocation } from './invocation';
import { InvokeOptions } from './invoke-options';

export interface InvokeRequest {
  /** List of Invocations */
  l: Invocation[];

  /** Options */
  o: InvokeOptions;
}
