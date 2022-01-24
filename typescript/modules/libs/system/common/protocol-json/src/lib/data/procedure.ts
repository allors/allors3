import { IUnit } from '@allors/system/workspace/domain';

export interface Procedure {
  /** Name */
  n: string;

  /** Collections */
  c: { [name: string]: number[] };

  /** Objects */
  o: { [name: string]: number };

  /** Values */
  v: { [name: string]: IUnit };

  /** Pool
   *  [][id,version]
   */
  p: number[][];
}
