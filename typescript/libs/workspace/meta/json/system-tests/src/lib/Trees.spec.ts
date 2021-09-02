import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';

describe('Trees', () => {
  const metaPopulation = new LazyMetaPopulation(data) as MetaPopulation;
  const m = metaPopulation as M;
  const { trees } = m;

  describe('with metadata', () => {
    it('should return nodes', () => {
      const tree = trees.Organisation({
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
