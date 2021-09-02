import { Strategy } from '../../session/Strategy';
import { Ranges } from './Ranges';

export class DefaultStrategyRanges extends Ranges<Strategy> {
  constructor() {
    super();
  }

  compare(x: Strategy, y: Strategy): number {
    if (x === y || (x == null && y == null)) {
      return 0;
    }

    return y == null ? 1 : x == null ? -1 : x.rangeId - y.rangeId;
  }
}
