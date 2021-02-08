import { MetaPopulation } from '@allors/meta/core';
import { Database, Session } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/workspace/memory';
import { Person } from '@allors/domain/generated';

import { data, Meta } from '@allors/meta/generated';

import { extend } from '@allors/domain/custom';

describe('Session', () => {
  let session: Session;
  let session2: Session;
  let m: Meta;

  beforeEach(() => {
    const metaPopulation = new MetaPopulation(data);
    const database: Database = new MemoryDatabase(metaPopulation);
    extend(database);

    m = metaPopulation as Meta;
    session = database.createSession();
    session2 = database.createSession();
  });

  describe('Create', () => {
    let person: Person;

    beforeEach(() => {
      person = session.create(m.Person) as Person;
    });

    it('can not instantiate in different session', () => {
      expect.toThrowWithMessage(()=> session2.get(person.id), 'Object with id -1 is not present.')
    });
  });
});
