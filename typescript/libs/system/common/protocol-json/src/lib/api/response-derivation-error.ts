import { DerivationRelation } from './derivation-relation';

export interface ResponseDerivationError {
  /** Message */
  m: string;

  /** Roles */
  r: DerivationRelation[];
}
