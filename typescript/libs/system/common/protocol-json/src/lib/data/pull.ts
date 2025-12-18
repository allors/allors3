import { Extent } from './extent';
import { Result } from './result';

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
