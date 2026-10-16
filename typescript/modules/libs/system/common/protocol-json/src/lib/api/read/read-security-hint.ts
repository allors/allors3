import { ReadSecurityHeader } from './read-security-header';

export interface ReadSecurityHint extends ReadSecurityHeader {
  /** Whether the exact body is held, independently of record provenance. */
  h: boolean;
}
