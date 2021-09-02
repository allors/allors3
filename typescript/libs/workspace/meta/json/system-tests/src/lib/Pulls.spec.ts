import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { M } from '@allors/workspace/meta/core';
import { data } from '@allors/workspace/meta/json/core';

describe('Pulls', () => {
  const metaPopulation = new LazyMetaPopulation(data) as MetaPopulation;
  const m = metaPopulation as M;
  const { pulls } = m;

  describe('with metadata', () => {
    it('should return pulls', () => {
      const pull = pulls.Organisation({
        select: {
          Owner: {
            OrganisationsWhereOwner: {
              include: {
                Shareholders: {},
              },
            },
          },
        },
      });

      expect(pull).toBeDefined();
    });
  });
});
