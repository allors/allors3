import { PushRequestRole } from './push-request-role';

export interface PushRequestObject {
  /** DatabaseId */
  d: number;

  /** Version */
  v: number;

  /** Roles */
  r?: PushRequestRole[];
}
