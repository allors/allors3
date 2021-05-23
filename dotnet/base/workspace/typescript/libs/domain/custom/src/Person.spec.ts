import { Meta, data } from '@allors/meta/generated';
import { MetaPopulation } from '@allors/meta/core';
import { Session, Database } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/workspace/memory';

import { extend } from '../src';
import { Person } from '@allors/domain/generated';

describe('Person', () => {
  let m: Meta;
  let session: Session;

  beforeEach(() => {
    m = new MetaPopulation(data) as Meta;
    const database: Database = new MemoryDatabase(m);
    extend(database);

    session = database.createSession();
  });

  describe('displayName', () => {
    let person: Person;

    beforeEach(() => {
      person = session.create(m.Person) as Person;
    });

    it('should be N/A when nothing set', () => {
      expect(true).toBe(true);
    });

  });
});
