import { Strategy } from '../../session/Strategy';
import { Ranges } from './Ranges';

export class DefaultStrategyRanges extends Ranges<Strategy> {
  constructor() {
    super();
  }

  compare(x: Strategy, y: Strategy): number {
    return x.rangeId - y.rangeId;
  }
}
