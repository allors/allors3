import { Composite, Unit, UnitTags, Multiplicity, Origin, RoleType } from '@allors/workspace/system';
import { LazyMetaPopulation } from '@allors/meta/lazy/system';

interface Organisation extends Composite {
  Name: RoleType;
}

interface M extends LazyMetaPopulation {
  String: Unit;

  Organisation: Organisation;
}

describe('MetaPopulation', () => {
  describe('with minimal unit relation metadata', () => {
    const metaPopulation = new LazyMetaPopulation({
      c: [[10, 'Organisation', [], [[11, UnitTags.String, 'Name']]]],
    }) as M;

    const { Organisation, String } = metaPopulation;
    const { Name: roleType } = Organisation;

    it('should have the relation with its defaults', () => {
      expect(roleType).toBeDefined();
      expect(roleType.objectType).toBe(String);

      const { relationType, associationType } = roleType;

      expect(relationType).toBeDefined;
      expect(relationType.multiplicity).toBe(Multiplicity.OneToOne);
      expect(relationType.origin).toBe(Origin.Database);
      expect(relationType.isDerived).toBeFalsy();
      expect(relationType).not.toBeNull();

      expect(associationType).toBeDefined;
      expect(associationType.objectType).toBe(Organisation);
    });
  });
});
