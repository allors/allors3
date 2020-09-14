import { MetaPopulation } from '@allors/meta/system';
import { Session, Database } from '@allors/workspace/system';
import { MemoryDatabase } from '@allors/workspace/memory';

import { data, Meta } from '@allors/meta/generated';
import { Organisation, Person} from '@allors/domain/generated';

import { syncResponse, securityResponse } from '../fixture';

import { extend } from '@allors/domain/custom';


import 'jest-extended';

describe('Session', () => {
  let m: Meta;
  let database: Database;

  beforeEach(() => {
    m = new MetaPopulation(data) as Meta;
    database = new MemoryDatabase(m);
    extend(database);

    database.sync(syncResponse(m));
    database.security(securityResponse(m));
  });

  describe('delete', () => {
    let session: Session;

    beforeEach(() => {
      session = database.createSession();
    });

    it('should throw exception for existing object', () => {
      const koen = session.get('1') as Person;
      expect(() => {
        session.delete(koen);
      }).toThrow();
    });

    it('should not throw exception for a new object', () => {
      const jos = session.create(m.Person) as Person;
      expect(() => {
        session.delete(jos);
      }).not.toThrow();
    });

    it('should throw exception for a deleted object', () => {
      const acme = session.create(m.Organisation) as Organisation;

      session.delete(acme);

      expect(acme.CanReadName).toBeUndefined();
      expect(acme.CanWriteName).toBeUndefined();
      expect(acme.Name).toBeUndefined();

      expect(() => {
        acme.Name = 'Acme';
      }).toThrow();

      const jos = session.create(m.Person) as Person;

      expect(acme.CanReadOwner).toBeUndefined();
      expect(acme.CanWriteOwner).toBeUndefined();
      expect(acme.Owner).toBeUndefined();

      expect(() => {
        acme.Owner = jos;
      }).toThrow();

      expect(acme.CanReadEmployees).toBeUndefined();
      expect(acme.CanWriteEmployees).toBeUndefined();
      expect(acme.Employees).toBeUndefined();

      expect(() => {
        acme.AddEmployee(jos);
      }).toThrow();

      expect(acme.CanExecuteJustDoIt).toBeUndefined();
      expect(acme.JustDoIt).toBeUndefined();
    });

    it('should delete role from existing object', () => {
      const acme = session.get('101') as Organisation;
      const jos = session.create(m.Person) as Person;

      acme.Owner = jos;

      session.delete(jos);

      expect(acme.Owner).toBeNull();
    });

    it('should remove role from existing object', () => {
      const acme = session.get('101') as Organisation;
      const jos = session.create(m.Person) as Person;

      acme.AddEmployee(jos);

      session.delete(jos);

      expect(acme.Employees).not.toContain(jos);
    });

    it('should delete role from new object', () => {
      const acme = session.create(m.Organisation) as Organisation;
      const jos = session.create(m.Person) as Person;

      acme.Owner = jos;

      session.delete(jos);

      expect(acme.Owner).toBeNull();
    });

    it('should remove role from new object', () => {
      const acme = session.create(m.Organisation) as Organisation;
      const jos = session.create(m.Person) as Person;

      acme.AddEmployee(jos);

      session.delete(jos);

      expect(acme.Employees).not.toContain( jos);
    });
  });
});
