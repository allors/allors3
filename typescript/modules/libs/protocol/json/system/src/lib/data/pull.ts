import { Extent } from "./Extent";
import { Result } from "./Result";

export interface Pull {
  /** ExtentRef */
  er?: string;

  /** Extent */
  e?: Extent;

  /** ObjectType */
  t?: string;

  /** Object */
  o?: number;

  /** Results */
  r?: Result[];

  /** Arguments */
  a?: { [name: string]: string };
}
