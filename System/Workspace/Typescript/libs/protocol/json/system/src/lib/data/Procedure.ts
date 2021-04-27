import { UnitTypes } from "@allors/workspace/system";

export interface Procedure {
  /** Name */
  n: string;

  /** Collections */
  c: { [name: string]: number[] };

  /** Objects */
  o: { [name: string]: number };

  /** Values */
  v: { [name: string]: UnitTypes };

  /** Pool
   *  [][id,version]
   */
  p: number[][];
}
