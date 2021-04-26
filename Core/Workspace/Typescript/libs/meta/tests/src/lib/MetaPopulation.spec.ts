import { LazyMetaPopulation } from '@allors/meta/lazy/system';
import { data, M } from '@allors/meta/generated';

describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new LazyMetaPopulation(data) as unknown as M;

    it('should be newable', () => {
      expect(metaPopulation).not.toBeNull();
    });

    describe('init with empty data population', () => {
      it('should contain Binary, Boolean, DateTime, Decimal, Float, Integer, String, Unique (from Core)', () => {
        ['Binary', 'Boolean', 'DateTime', 'Decimal', 'Float', 'Integer', 'String', 'Unique'].forEach((name) => {
          const unit = metaPopulation[name];
          expect(unit).not.toBeNull();
        });
      });

      it('should contain Media, ObjectState, Counter, Person, Role, UserGroup (from Core)', () => {
        ['Media', 'ObjectState', 'Counter', 'Person', 'Role', 'UserGroup'].forEach((name) => {
          const unit = metaPopulation[name];
          expect(unit).not.toBeNull();
        });
      });
    });
  });
});
