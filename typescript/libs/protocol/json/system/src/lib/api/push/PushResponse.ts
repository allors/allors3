import { PushResponseNewObject } from "./PushResponseNewObject";

export interface PushResponse extends Response {
  /** NewObjects */
  n: PushResponseNewObject[];
}
