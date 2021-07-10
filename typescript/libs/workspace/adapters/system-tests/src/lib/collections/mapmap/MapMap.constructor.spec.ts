import { MapMap, Range } from '@allors/workspace/adapters/system';

describe('MapMap', () => {
  describe('after construction', () => {
    const mapMap = new MapMap<string, string, Range>();

    it('should be defined', () => {
      expect(mapMap).toBeDefined();
    });
  });
});
