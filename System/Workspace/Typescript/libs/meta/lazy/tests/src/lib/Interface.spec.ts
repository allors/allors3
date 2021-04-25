import { Interface, MetaData } from '@allors/workspace/system';
import { LazyMetaPopulation } from '@allors/meta/lazy/system';

type Named = Interface;

interface M extends LazyMetaPopulation {
  Named: Named;
}

describe('MetaPopulation', () => {
  describe('constructor with minimal interface metadata', () => {
    const data: MetaData = {
      i: [[9, 'Named']],
    };

    const metaPopulation = new LazyMetaPopulation(data) as M;

    it('should have the interface', () => {
      expect(metaPopulation.Named).not.toBeNull();
    });

    describe('with interface', () => {
      const named = metaPopulation.Named;
      it('should have properties ', () => {
        expect(named.tag).toBe(9);
        expect(named.singularName).toBe('Named');
      });
    });
  });
});
