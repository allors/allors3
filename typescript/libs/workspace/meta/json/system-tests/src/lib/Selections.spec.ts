import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';

describe('Selections', () => {
  const metaPopulation = new LazyMetaPopulation(data) as MetaPopulation;
  const m = metaPopulation as M;
  const { selections } = m;

  describe('with metadata', () => {
    it('should return selections', () => {
      const selection = selections.Organisation({
        Owner: {
          OrganisationsWhereOwner: {
            include: {
              Shareholders: {},
            },
          },
        },
      });

      expect(selection).toBeDefined();
      expect(selection.propertyType).toBe(m.Organisation.Owner);
      expect(selection.include).toBeUndefined();

      const next = selection.next;

      expect(next).toBeDefined();
      expect(next.propertyType).toBe(m.Person.OrganisationsWhereOwner);
      expect(next.include).toBeUndefined();

      const nextnext = next.next;
      expect(nextnext).toBeDefined();
      expect(nextnext.propertyType).toBeUndefined();
      expect(nextnext.include).toBeDefined();

      const include = nextnext.include;

      expect(include.length).toBe(1);
      expect(include[0].propertyType).toBe(m.Organisation.Shareholders);
      expect(include[0].nodes).toBeUndefined();
    });
  });
});
