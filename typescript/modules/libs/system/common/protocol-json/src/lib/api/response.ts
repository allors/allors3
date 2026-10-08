import { ResponseDerivationError } from './response-derivation-error';

export interface Response {
  /** error message */
  _e: string;

  /** version errors */
  _v: number[];

  /** access errors */
  _a: number[];

  /** missing errors */
  _m: number[];

  /** derivation errors */
  _d: ResponseDerivationError[];

  /** The id of the database the server serves */
  _db?: string;

  /** The id of the user the server served the request as */
  _u?: number;

  /** The name of the workspace the server served */
  _w?: string;

  /** The fingerprint of the server's meta for that workspace */
  _f?: string;
}
