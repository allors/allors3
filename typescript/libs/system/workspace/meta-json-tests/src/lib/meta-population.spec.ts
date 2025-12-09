import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';

describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new LazyMetaPopulation({}) as MetaPopulation;

    it('should be newable', () => {
      expect(metaPopulation).toBeDefined();
    });
  });
});
