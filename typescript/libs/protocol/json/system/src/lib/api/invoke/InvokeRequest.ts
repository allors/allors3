import { Invocation } from "./Invocation";
import { InvokeOptions } from "./InvokeOptions";

export interface InvokeRequest {
  /** List of Invocations */
  l: Invocation[];

  /** Options */
  o: InvokeOptions;
}
