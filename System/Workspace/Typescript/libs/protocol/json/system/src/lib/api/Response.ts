import { ResponseDerivationError } from './ResponseDerivationError';

export interface Response {
  /** error message */
  e: string;

  /** version errors */
  v: number[];

  /** access errors */
  a: number[];

  /** missing errors */
  m: number[];

  /** derivation errors */
  d: ResponseDerivationError[];
}
