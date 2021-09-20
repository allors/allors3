import { Ranges } from './Ranges';
export class DefaultNumberRanges extends Ranges<number> {
  constructor() {
    super();
  }

  compare(x: number, y: number): number {
    return x - y;
  }
}
