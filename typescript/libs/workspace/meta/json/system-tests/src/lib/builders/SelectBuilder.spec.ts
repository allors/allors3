import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';

describe('SelectBuilder', () => {
  const metaPopulation = new LazyMetaPopulation(data) as MetaPopulation;
  const m = metaPopulation as M;
  const { selectBuilder: s } = m;

  describe('with metadata', () => {
    it('should return selectBuilder', () => {
      const selection = s.Organisation({
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
      expect(next.next).toBeUndefined();
      expect(next.propertyType).toBe(m.Person.OrganisationsWhereOwner);
      expect(next.include).toBeDefined();

      const include = next.include;

      expect(include.length).toBe(1);
      expect(include[0].propertyType).toBe(m.Organisation.Shareholders);
      expect(include[0].nodes).toBeUndefined();
    });
  });
});
