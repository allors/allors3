import { importFrom } from '@allors/workspace/adapters/system';

describe('Range', () => {
  describe('initialize with undefined', () => {
    const set = importFrom(undefined);

    it('should return undefined', () => {
      expect(set).toBeUndefined();
    });
  });
});
