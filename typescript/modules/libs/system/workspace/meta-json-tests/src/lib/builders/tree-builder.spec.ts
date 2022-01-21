import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { M } from '@allors/core/workspace/meta';
import { data } from '@allors/core/workspace/meta-json';

describe('TreeBuilder', () => {
  const metaPopulation = new LazyMetaPopulation(data) as MetaPopulation;
  const m = metaPopulation as M;
  const { treeBuilder: t, dependency: d } = m;

  describe('with metadata', () => {
    it('should return nodes', () => {
      const tree = t.Organisation({
        Owner: {
          OrganisationsWhereOwner: {},
        },
      });

      expect(tree).toBeDefined();
      expect(tree.length).toBe(1);

      const node = tree[0];

      expect(node.propertyType).toBe(m.Organisation.Owner);
      expect(node.nodes.length).toBe(1);

      const subnode = node.nodes[0];
      expect(subnode.propertyType).toBe(m.Person.OrganisationsWhereOwner);
      expect(subnode.nodes).toBeUndefined();
    });
  });
});
