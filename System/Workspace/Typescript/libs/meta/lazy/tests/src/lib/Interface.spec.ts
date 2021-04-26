import { Interface, MetaData } from '@allors/workspace/system';
import { LazyMetaPopulation } from '@allors/meta/lazy/system';

type Named = Interface;

interface M extends LazyMetaPopulation {
  Named: Named;
}

describe('MetaPopulation', () => {
  describe('with minimal interface metadata', () => {
    const data: MetaData = {
      i: [[9, 'Named']],
    };

    const metaPopulation = new LazyMetaPopulation(data) as M;
    const { Named } = metaPopulation;

    it('should have the interface with its defaults', () => {
      expect(Named).toBeDefined();
      expect(Named.tag).toBe(9);
      expect(Named.singularName).toBe('Named');
    });
  });
});
