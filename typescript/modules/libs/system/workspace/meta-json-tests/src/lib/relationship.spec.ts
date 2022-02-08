import { Composite, Unit, RoleType } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';

describe('Relationship in MetaPopulation', () => {
  describe('with minimal metadata', () => {
    interface Employment extends Composite {
      Employer: RoleType;
      Employee: RoleType;
    }

    type Party = Composite;

    interface M extends LazyMetaPopulation {
      String: Unit;

      Employment: Employment;
      Party: Party;
    }

    const metaPopulation = new LazyMetaPopulation({
      i: [['10', 'Party']],
      c: [
        [
          '11',
          'Employment',
          [],
          [
            ['20', '10', 'Employer'],
            ['21', '10', 'Employee'],
          ],
        ],
      ],
      rel: ['20', '21'],
    }) as M;

    const { Employment, Party } = metaPopulation;
    const { Employer, Employee } = Employment;

    it('should have the relation with its defaults', () => {
      expect(Employment).toBeDefined();
      expect(Employment.isRelationship).toBeTruthy();

      expect(Employer).toBeDefined;
      expect(Employer.relationType.inRelationship).toBeTruthy;

      expect(Employee).toBeDefined;
      expect(Employee.relationType.inRelationship).toBeTruthy;
    });
  });
});
