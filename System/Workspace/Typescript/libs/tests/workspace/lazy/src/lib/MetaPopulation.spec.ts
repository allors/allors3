import { MetaData, MetaPopulation } from '@allors/workspace/system';
import { LazyMetaPopulation } from '@allors/workspace/lazy';


describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new LazyMetaPopulation({}) as MetaPopulation;

    it('should be newable', () => {
      expect(metaPopulation).not.toBeNull();
    });

  });
});
