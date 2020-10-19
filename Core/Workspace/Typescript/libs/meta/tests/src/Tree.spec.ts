import { MetaPopulation } from '@allors/meta/core';
import { data, TreeFactory, Meta } from '@allors/meta/generated';

import 'jest-extended';

describe('Tree', () => {
  let m: Meta;
  let factory: TreeFactory;

  beforeEach(async () => {
    m = new MetaPopulation(data) as Meta;
    factory = new TreeFactory(m);
  });

  describe('with empty include', () => {
    it('should serialize to correct json', () => {
      const orignal = factory.Organisation({});

      const json = JSON.stringify(orignal);
      const include = JSON.parse(json);

      expect(include).toBeArray();
      expect(include).toBeEmpty();
    });
  });

  describe('with one role include', () => {
    it('should serialize to correct json', () => {
      const original = factory.Organisation({
        Employees: {},
      });

      const json = JSON.stringify(original);
      const include = JSON.parse(json);

      expect(include).toEqual([
        {
          roleType: m.Organisation.Employees.relationType.id,
        },
      ]);
    });
  });

  describe('with two roles include', () => {
    it('should serialize to correct json', () => {
      const original = factory.Organisation({
        Employees: {},
        Manager: {},
      });

      const json = JSON.stringify(original);
      const include = JSON.parse(json);

      expect(include).toEqual([
        {
          roleType: m.Organisation.Employees.relationType.id,
        },
        {
          roleType: m.Organisation.Manager.relationType.id,
        },
      ]);
    });
  });

  describe('with a nested role include', () => {
    it('should serialize to correct json', () => {
      const original = factory.Organisation({
        Employees: {
          CycleOne: {},
        },
      });

      const json = JSON.stringify(original);
      const include = JSON.parse(json);

      expect(include).toEqual([
        {
          nodes: [
            {
              roleType: m.Person.CycleOne.relationType.id,
            },
          ],
          roleType: m.Organisation.Employees.relationType.id,
        },
      ]);
    });
  });

  describe('with a subclass role include', () => {
    it('should serialize to correct json', () => {
      const original = factory.Deletable({
        Person_CycleOne: {},
      });

      const json = JSON.stringify(original);
      const include = JSON.parse(json);

      expect(include).toEqual([
        {
          roleType: m.Person.CycleOne.relationType.id,
        },
      ]);
    });
  });

  describe('with a non exsiting role include', () => {
    it('should throw exception', () => {
      expect(() => {
        const original = factory.Organisation({
          Oops: {},
        } as any);
      }).toThrowError(Error);
    });
  });
});
