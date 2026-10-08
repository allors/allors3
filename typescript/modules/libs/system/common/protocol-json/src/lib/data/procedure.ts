export interface Procedure {
  /** Name */
  n: string;

  /** Collections */
  c: { [name: string]: number[] };

  /** Objects */
  o: { [name: string]: number };

  /** Values */
  v: { [name: string]: unknown };

  /** Pool
   *  [][id,version]
   */
  p: number[][];
}
