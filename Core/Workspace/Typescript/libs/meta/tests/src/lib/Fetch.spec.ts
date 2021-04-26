import { MetaPopulation } from '@allors/meta/core';
import { Database } from '@allors/workspace/core';

import { data, SelectFactory, Meta } from '@allors/meta/generated';
import { MemoryDatabase } from '@allors/adapters/memory/core';

describe('Select', () => {
  let m: Meta;
  let factory: SelectFactory;
  let database: Database;

  beforeEach(async () => {
    m = new MetaPopulation(data) as Meta;
    database = new MemoryDatabase(m);

    factory = new SelectFactory(m);
  });

  // describe('with empty select',
  //     () => {
  //         it('should serialize to correct json', () => {

  //             const original = factory.Organisation({});

  //             const json = JSON.stringify(original);
  //             const select = JSON.parse(json);

  //             expect(select).toBeDefined();
  //         });
  //     });

  // describe('with one role select',
  //     () => {
  //         it('should serialize to correct json', () => {

  //             const original = factory.Organisation({
  //                 Employees: {},
  //             });

  //             const json = JSON.stringify(original);
  //             const select = JSON.parse(json);

  //             assert.deepEqual(select, { step: { propertytype: 'b95c7b34-a295-4600-82c8-826cc2186a00' } });
  //         });
  //     });

  // describe('with two roles select',
  //     () => {
  //         it('should serialize to correct json', () => {

  //             const original =
  //                 factory.Organisation({
  //                     Employees: {
  //                         Photo: {},
  //                     },
  //                 });

  //             const json = JSON.stringify(original);
  //             const select = JSON.parse(json);

  //             assert.deepEqual(select, {
  //                 step: {
  //                     next: {
  //                         propertytype: 'f6624fac-db8e-4fb2-9e86-18021b59d31d',
  //                     },
  //                     propertytype: 'b95c7b34-a295-4600-82c8-826cc2186a00',
  //                 }
  //             });

  //         });
  //     });

  describe('with a subclass role select', () => {
    it('should serialize to correct json', () => {
      const original = factory.User({
        Person_CycleOne: {},
      });

      const json = JSON.stringify(original);
      const select = JSON.parse(json);

      expect(select).toEqual({
        step: { roleType: m.Person.CycleOne.relationType.id },
      });
    });
  });

  // describe('with a non exsiting role select',
  //     () => {
  //         it('should throw exception', () => {

  //             assert.throw(() => {
  //                 factory.Organisation({
  //                     Oops: {},
  //                 } as any);
  //             }, Error);
  //         });
  //     });

  // describe('with one association select',
  //     () => {
  //         it('should serialize to correct json', () => {

  //             const original = factory.Organisation({
  //                 PeopleWhereCycleOne: {},
  //             });

  //             const json = JSON.stringify(original);
  //             const select = JSON.parse(json);

  //             assert.deepEqual(select, { step: { propertytype: 'dec66a7b-56f5-4010-a2e7-37e25124bc77' } });
  //         });
  //     });

  describe('with one subclass association select', () => {
    it('should serialize to correct json', () => {
      const orginal = factory.Deletable({
        Organisation_PeopleWhereCycleOne: {},
      });

      const json = JSON.stringify(orginal);
      const select = JSON.parse(json);

      expect(select).toEqual({
        step: {
          associationType: m.Organisation.PeopleWhereCycleOne.relationType.id,
        },
      });
    });
  });
});
