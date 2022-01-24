import { IObject } from '@allors/system/workspace/domain';
import { Strategy } from '../../session/strategy';
import { Ranges } from './ranges';

export class DefaultObjectRanges extends Ranges<IObject> {
  constructor() {
    super();
  }

  compare(x: IObject, y: IObject): number {
    if (x === y || (x == null && y == null)) {
      return 0;
    }

    return y == null
      ? 1
      : x == null
      ? -1
      : (x.strategy as Strategy).rangeId - (y.strategy as Strategy).rangeId;
  }
}
