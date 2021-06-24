import { PushResponseNewObject } from "./PushResponseNewObject";
import { Response } from '../Response'

export interface PushResponse extends Response {
  /** NewObjects */
  n: PushResponseNewObject[];
}
