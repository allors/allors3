import { MapMap, IRange } from '@allors/workspace/adapters/system';

describe('MapMap', () => {
  describe('after construction', () => {
    const mapMap = new MapMap<string, string, IRange>();

    it('should be defined', () => {
      expect(mapMap).toBeDefined();
    });
  });
});
