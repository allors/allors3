import { IUnit } from '@allors/system/workspace/domain';

export interface PullArgs {
  /** Collections */
  c: { [name: string]: number[] };

  /** Objects */
  o: { [name: string]: number };

  /** Values */
  v: { [name: string]: IUnit };

  /** Pool */
  p: number[][];
}
