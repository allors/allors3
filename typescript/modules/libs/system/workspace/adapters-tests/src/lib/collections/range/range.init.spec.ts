import { DefaultNumberRanges } from '@allors/workspace/adapters/system';

describe('Range', () => {
  describe('initialize with undefined', () => {
    const ranges = new DefaultNumberRanges();
    const set = ranges.importFrom(undefined);

    it('should return undefined', () => {
      expect(set).toBeUndefined();
    });
  });
});
