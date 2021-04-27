import { PullResponseObject } from "./PullResponseObject";

export interface PullResponse {
  /** Collections */
  c: { [name: string]: number[] };

  /** Objects */
  o: { [name: string]: number };

  /** Values */
  v: { [name: string]: string };

  /** Pool */
  p: PullResponseObject[];

  /** AccessControls */
  a: number[][];
}
