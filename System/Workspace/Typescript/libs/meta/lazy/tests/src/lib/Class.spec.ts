import { Composite, Unit } from '@allors/workspace/system';
import { LazyMetaPopulation } from '@allors/meta/lazy/system';
import { UnitTags } from '../../../../../workspace/system/src/lib/UnitTags';

type Organisation = Composite;

interface M extends LazyMetaPopulation {
  Organisation: Organisation;
}

describe('MetaPopulation', () => {
  describe('with minimal class metadata', () => {
    const metaPopulation = new LazyMetaPopulation({
      c: [[10, 'Organisation']],
    }) as M;

    const { Organisation } = metaPopulation;

    it('should have the class with its defaults', () => {
      expect(Organisation).toBeDefined();
      expect(Organisation.tag).toBe(10);
      expect(Organisation.singularName).toBe('Organisation');
      expect(Organisation.pluralName).toBe('Organisations');
    });
  });
});
