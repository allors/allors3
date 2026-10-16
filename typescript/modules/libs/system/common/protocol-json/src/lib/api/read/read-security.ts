import { ReadSecurityHeader } from './read-security-header';
import { ReadSecurityPayload } from './read-security-payload';

export interface ReadSecurity extends ReadSecurityHeader {
  /** Full security body or exact request-security reuse. */
  p: ReadSecurityPayload;
}
