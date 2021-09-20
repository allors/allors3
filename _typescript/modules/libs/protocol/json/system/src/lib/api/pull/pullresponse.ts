import { PullResponseObject } from "./PullResponseObject";
import { Response } from '../Response'

export interface PullResponse extends Response {
  /** Collections */
  c: { [name: string]: number[] };

  /** Objects */
  o: { [name: string]: number };

  /** Values */
  v: { [name: string]: string };

  /** Pool */
  p: PullResponseObject[];

  /** Grants [id, version] */
  g: number[][];

  /** Revocations [id, version] */
  r: number[][];
}
