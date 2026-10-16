import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { M } from '@allors/core/workspace/meta';
import { data } from '@allors/core/workspace/meta-json';
import { Filter } from '@allors/system/workspace/domain';

describe('Pulls', () => {
  const metaPopulation = new LazyMetaPopulation(data) as MetaPopulation;
  const m = metaPopulation as M;
  const { pullBuilder: p } = m;

  describe('with metadata', () => {
    it('should build named inline results with includes and arguments', () => {
      const pull = p.Organisation({
        arguments: { name: 'Acme' },
        results: [
          {
            name: 'owners',
            select: {
              Owner: { include: { OrganisationsWhereOwner: {} } },
            },
            skip: 2,
            take: 3,
          },
          { name: 'organisations', include: { Owner: {} } },
        ],
      });

      expect((pull.extent as Filter).objectType).toBe(m.Organisation);
      expect(pull.arguments).toEqual({ name: 'Acme' });
      expect(Object.keys(pull)).not.toContain('extentRef');
      expect(pull.results).toHaveLength(2);
      const [owners, organisations] = pull.results;
      expect(owners.name).toBe('owners');
      expect(owners.skip).toBe(2);
      expect(owners.take).toBe(3);
      expect(owners.select.propertyType).toBe(m.Organisation.Owner);
      expect(owners.select.include[0].propertyType).toBe(
        m.Person.OrganisationsWhereOwner
      );
      expect(organisations.name).toBe('organisations');
      expect(organisations.include[0].propertyType).toBe(m.Organisation.Owner);
      for (const result of pull.results) {
        expect(Object.keys(result)).not.toContain('selectRef');
      }
    });

    it('should return pulls', () => {
      const pull = p.Organisation({
        select: {
          Owner: {
            include: {
              OrganisationsWhereOwner: {},
            },
          },
        },
        skip: 20,
        take: 10,
      });

      expect(pull).toBeDefined();
      expect(pull.extent).toBeDefined();
      expect(pull.extent.kind).toBe('Filter');

      const extent = pull.extent as Filter;

      expect(extent.objectType).toBe(m.Organisation);

      expect(pull.results).toBeDefined();
      expect(pull.results.length).toBe(1);

      const result = pull.results[0];

      expect(result.name).toBeUndefined();
      expect(result.select).toBeDefined();
      expect(result.skip).toBe(20);
      expect(result.take).toBe(10);

      const select = result.select;

      expect(select.include).toBeDefined();
      expect(select.next).toBeUndefined();
      expect(select.propertyType).toBe(m.Organisation.Owner);

      const include = select.include;

      expect(include.length).toBe(1);

      const node = include[0];

      expect(node.nodes).toBeUndefined();
      expect(node.ofType).toBeUndefined();
      expect(node.propertyType).toBe(m.Person.OrganisationsWhereOwner);
    });
  });
});
