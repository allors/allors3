import { MetaPopulation } from '@allors/meta/system';
import { Database, Session } from '@allors/domain/system';
import { MemoryDatabase } from '@allors/domain/memory';
import { Person } from '@allors/session/generated';

import { data, Meta } from '@allors/meta/generated';

import { extend } from '../index';

describe('Person', () => {
  let session: Session;
  let m: Meta;

  beforeEach(() => {
    const metaPopulation = new MetaPopulation(data);
    const database: Database = new MemoryDatabase(metaPopulation);
    extend(database);

    m = metaPopulation as Meta;
    session = database.createSession();
  });

  describe('UserName', () => {
    let person: Person;

    beforeEach(() => {
      person = session.create(m.Person) as Person;
    });

    it('should have a UserName', () => {
      const userName = person.UserName;
    });
  });

  xdescribe('displayName', () => {
    let person: Person;

    beforeEach(() => {
      person = session.create(m.Person) as Person;
    });

    it('should be N/A when nothing set', () => {
      expect(person.DisplayName).toBe('N/A');
    });

    it('should be john@doe.com when username is john@doe.com', () => {
      person.UserName = 'john@doe.com';
      expect(person.DisplayName).toBe('john@doe.com');
    });

    it('should be Doe when lastName is Doe', () => {
      person.LastName = 'Doe';
      expect(person.DisplayName).toBe('Doe');
    });

    it('should be John with firstName John', () => {
      person.FirstName = 'John';
      expect(person.DisplayName).toBe('John');
    });

    it('should be John Doe (even twice) with firstName John and lastName Doe', () => {
      person.FirstName = 'John';
      person.LastName = 'Doe';

      expect(person.DisplayName).toBe('John Doe');
      expect(person.DisplayName).toBe('John Doe');
    });
  });
});
