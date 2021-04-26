import { Composite, Unit, UnitTags, Multiplicity, Origin, RoleType, Interface } from '@allors/workspace/system';
import { LazyMetaPopulation } from '@allors/meta/lazy/system';

interface Named extends Interface {
  Name: RoleType;
}

interface Organisation extends Composite {
  Name: RoleType;
}

interface M extends LazyMetaPopulation {
  String: Unit;

  Named: Named;

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
      expect(roleType.isOne).toBeTruthy();
      expect(roleType.isMany).toBeFalsy();
      expect(roleType.origin).toBe(Origin.Database);
      expect(roleType.isRequired).toBeFalsy();
      expect(roleType.isUnique).toBeFalsy();
      expect(roleType.size).toBeUndefined();
      expect(roleType.scale).toBeUndefined();
      expect(roleType.precision).toBeUndefined();
      expect(roleType.mediaType).toBeUndefined();

      const { relationType, associationType } = roleType;

      expect(relationType).toBeDefined;
      expect(relationType.multiplicity).toBe(Multiplicity.OneToOne);
      expect(relationType.origin).toBe(Origin.Database);
      expect(relationType.isDerived).toBeFalsy();
      expect(relationType).not.toBeNull();

      expect(associationType).toBeDefined;
      expect(associationType.objectType).toBe(Organisation);
      expect(associationType.isOne).toBeTruthy();
      expect(associationType.isMany).toBeFalsy();
      expect(associationType.origin).toBe(Origin.Database);
    });
  });

  describe('with inherited unit relation metadata', () => {
    const metaPopulation = new LazyMetaPopulation({
      i: [[9, 'Named', [], [[11, UnitTags.String, 'Name']]]],
      c: [[10, 'Organisation', [9]]],
    }) as M;

    const { Named, Organisation} = metaPopulation;
    const { Name: namedRoleType } = Named;
    const { Name: organisationRoleType } = Organisation;

    it('should have the same RoleType', () => {
      expect(namedRoleType).toBeDefined();
      expect(organisationRoleType).toBeDefined();
      expect(organisationRoleType).toEqual(namedRoleType);
    });
  });
});
