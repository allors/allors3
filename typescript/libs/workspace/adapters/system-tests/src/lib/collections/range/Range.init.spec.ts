import { fromUnsorted } from '@allors/workspace/adapters/system';

describe('Range', () => {
  describe('initialize with undefined', () => {
    const set = fromUnsorted(undefined);

    it('should return undefined', () => {
      expect(set).toBeUndefined();
    });
  });
});
