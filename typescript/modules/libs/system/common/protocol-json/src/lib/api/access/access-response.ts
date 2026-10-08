import { Response } from '../response';
import { AccessResponseGrant } from './access-response-grant';
import { AccessResponseRevocation } from './access-response-revocation';

export interface AccessResponse extends Response {
  /** Grants */
  g: AccessResponseGrant[];

  /** Revocations */
  r: AccessResponseRevocation[];
}
