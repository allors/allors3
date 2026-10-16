import { DerivationRelation } from './derivation-relation';

export interface ResponseDerivationError {
  /** Message */
  m?: string | null;

  /** Roles */
  r: DerivationRelation[];
}
