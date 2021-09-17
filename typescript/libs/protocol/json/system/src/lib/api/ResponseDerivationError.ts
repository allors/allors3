import { DerivationRole } from './DerivationRole';

export interface ResponseDerivationError {
  /** Message */
  m: string;

  /** Roles */
  r: DerivationRole[];
}
