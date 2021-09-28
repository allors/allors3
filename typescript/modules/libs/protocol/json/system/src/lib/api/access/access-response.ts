import { AccessResponseGrant } from './access-response-grant';
import { AccessResponseRevocation } from './access-response-revocation';

export interface AccessResponse {
  /** Grants */
  g: AccessResponseGrant[];

  /** Grants */
  r: AccessResponseRevocation[];
}
