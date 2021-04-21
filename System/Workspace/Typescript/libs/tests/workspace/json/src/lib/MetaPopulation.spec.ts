import { IMetaPopulation, IUnit } from '@allors/workspace/system';
import { MetaPopulation } from '@allors/workspace/json';

describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new MetaPopulation({}) as IMetaPopulation;

    it('should be newable', () => {
      expect(metaPopulation).not.toBeNull();
    });

  });
});
