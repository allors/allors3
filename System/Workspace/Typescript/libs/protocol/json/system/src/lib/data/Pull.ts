import { Result } from "./Result";

export interface Pull {
  /** ExtentRef */
  er: string;

  /** Extent */
  e: string;

  /** ObjectType */
  t: number;

  /** Object */
  o: number;

  /** Results */
  r: Result[];

  /** Parameters */
  p: { [name: string]: string };
}
