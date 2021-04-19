import { MetaPopulation } from '@allors/workspace/lazy';

describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new MetaPopulation();

    it('should be newable', () => {
      expect(metaPopulation).not.toBeNull();
    });
  });
});
