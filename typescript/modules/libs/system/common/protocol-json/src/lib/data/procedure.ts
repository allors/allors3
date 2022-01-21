import { IUnit } from '@allors/workspace/domain/system';

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
