import { LazyMetaPopulation } from '@allors/meta/lazy/system';
import { data, M } from '@allors/meta/generated';
import { Database } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/adapters/memory/core';

describe('Filter', () => {
  let m: M;
  let database: Database;

  beforeEach(async () => {
    m = (new LazyMetaPopulation(data) as unknown) as M;
    database = new MemoryDatabase(m);
  });

  // describe('with empty flatPull', () => {
  //   it('should serialize to correct json', () => {
  //     const original = factory.Organisation({});

  //     const json = JSON.stringify(original);
  //     const pull = JSON.parse(json);

  //     expect(pull).toBeDefined();
  //   });
  // });
});
