import { DerivationRelation } from "./DerivationRelation";

export interface ResponseDerivationError {
  /** Message */
  m: string;

  /** Roles */
  r: DerivationRelation[];
}
