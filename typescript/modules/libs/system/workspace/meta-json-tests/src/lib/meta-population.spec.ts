import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';

describe('MetaPopulation', () => {
  describe('default constructor', () => {
    const metaPopulation = new LazyMetaPopulation({}) as MetaPopulation;

    it('should be newable', () => {
      expect(metaPopulation).toBeDefined();
    });
  });

  describe('with classes that declare relation types and method types', () => {
    const metaPopulation = new LazyMetaPopulation({
      c: [
        [
          '10',
          'Organisation',
          [],
          [
            ['20', '7', 'Name'],
            ['21', '11', 'Owner'],
          ],
          [['30', 'JustDoIt']],
        ],
        ['11', 'Person', [], [['22', '7', 'FirstName']], [['31', 'Greet']]],
      ],
    }) as MetaPopulation;

    // The population's own sets hold every relation type and method type of its composites,
    // each once: the fingerprint of the workspace meta hashes them with the composites.
    it('holds the relation types of its composites', () => {
      const ofComposites = new Set(
        [...metaPopulation.composites].flatMap((v) =>
          [...v.roleTypes].map((w) => w.relationType)
        )
      );

      expect(metaPopulation.relationTypes.size).toBe(3);
      expect(metaPopulation.relationTypes).toEqual(ofComposites);
      for (const relationType of metaPopulation.relationTypes) {
        expect(metaPopulation.metaObjectByTag.get(relationType.tag)).toBe(
          relationType
        );
      }
    });

    it('holds the method types of its composites', () => {
      const ofComposites = new Set(
        [...metaPopulation.composites].flatMap((v) => [...v.methodTypes])
      );

      expect(metaPopulation.methodTypes.size).toBe(2);
      expect(metaPopulation.methodTypes).toEqual(ofComposites);
      for (const methodType of metaPopulation.methodTypes) {
        expect(metaPopulation.metaObjectByTag.get(methodType.tag)).toBe(
          methodType
        );
      }
    });
  });
});
