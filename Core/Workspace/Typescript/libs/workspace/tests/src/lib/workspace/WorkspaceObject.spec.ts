import { MetaPopulation } from '@allors/meta/core';
import { Database, Session } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/adapters/memory/core';
import { data, Meta } from '@allors/meta/generated';
import { WorkspacePerson } from '@allors/domain/generated';

import { extend } from '@allors/domain/custom';

describe('WorkspaceObject', () => {
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

  describe('create object', () => {
    let workspacePerson: WorkspacePerson;

    beforeEach(() => {
      workspacePerson = session.create(m.WorkspacePerson) as WorkspacePerson;
    });

    it('should exist', () => {
      expect(workspacePerson).toBeDefined();
    });

    describe('instantiate in same session', () => {
      let workspacePerson2: WorkspacePerson;

      beforeEach(() => {
        workspacePerson2 = session.get(workspacePerson.id) as WorkspacePerson;
      });

      it('should be the same object', () => {
        expect(workspacePerson2).toEqual(workspacePerson);
      });
    });

    describe('instantiate in different session', () => {
      let workspacePerson2: WorkspacePerson;

      beforeEach(() => {
        workspacePerson2 = session2.get(workspacePerson.id) as WorkspacePerson;
      });

      it('should be a different object', () => {
        expect(workspacePerson2).not.toEqual(workspacePerson);
      });

      it('should have a different session', () => {
        expect(workspacePerson2.session).not.toEqual(workspacePerson.session);
      });

      it('should have the same id', () => {
        expect(workspacePerson2.id).toEqual(workspacePerson.id);
      });

      it('should have the same objectType', () => {
        expect(workspacePerson2.objectType).toEqual(workspacePerson.objectType);
      });
   });
  });

  describe('WorkspaceUnitRole', () => {
    describe('set FirstName', () => {
      let workspacePerson: WorkspacePerson;

      beforeEach(() => {
        workspacePerson = session.create(m.WorkspacePerson) as WorkspacePerson;
        workspacePerson.FirstName = 'Jos';
      });

      it('should be the same', () => {
        const workspacePerson2 = session2.get(workspacePerson.id) as WorkspacePerson;
        expect(workspacePerson2.FirstName).toEqual('Jos');
      });

      describe('instantiate in same session', () => {
        let workspacePerson2: WorkspacePerson;

        beforeEach(() => {
          workspacePerson2 = session.get(workspacePerson.id) as WorkspacePerson;
        });

        it('should be the same', () => {
          expect(workspacePerson2.FirstName).toEqual('Jos');
        });
      });

      describe('instantiate in different session', () => {
        let workspacePerson2: WorkspacePerson;

        beforeEach(() => {
          workspacePerson2 = session2.get(workspacePerson.id) as WorkspacePerson;
        });

        it('should be the same', () => {
          expect(workspacePerson2.FirstName).toEqual('Jos');
        });
      });
    });
  });

  describe('WorkspaceCompositeRole', () => {
    describe('set FirstName', () => {
      let workspacePerson: WorkspacePerson;

      beforeEach(() => {
        workspacePerson = session.create(m.WorkspacePerson) as WorkspacePerson;
        workspacePerson.FirstName = 'Jos';
      });

      it('should be the same', () => {
        const workspacePerson2 = session2.get(workspacePerson.id) as WorkspacePerson;
        expect(workspacePerson2.FirstName).toEqual('Jos');
      });

      describe('instantiate in same session', () => {
        let workspacePerson2: WorkspacePerson;

        beforeEach(() => {
          workspacePerson2 = session.get(workspacePerson.id) as WorkspacePerson;
        });

        it('should be the same', () => {
          expect(workspacePerson2.FirstName).toEqual('Jos');
        });
      });

      describe('instantiate in different session', () => {
        let workspacePerson2: WorkspacePerson;

        beforeEach(() => {
          workspacePerson2 = session2.get(workspacePerson.id) as WorkspacePerson;
        });

        it('should be the same', () => {
          expect(workspacePerson2.FirstName).toEqual('Jos');
        });
      });
    });
  });
});
