import { MetaPopulation } from '@allors/meta/system';
import { Database, Session } from '@allors/workspace/system';
import { MemoryDatabase } from '@allors/workspace/memory';
import { Person } from '@allors/domain/generated';

import { data, Meta } from '@allors/meta/generated';

import { extend } from '../index';
import { WorkspacePerson } from '../../../generated/src/WorkspacePerson.g';

describe('Workspace', () => {
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

  describe('firstName', () => {
    let workspacePerson: WorkspacePerson;

    beforeEach(() => {
      workspacePerson = session.create(m.WorkspacePerson) as WorkspacePerson;
    });

    xit('get in different session', () => {
      workspacePerson.FirstName = "Jos";

      const workspacePerson2 = session2.get(workspacePerson.id) as WorkspacePerson;
      expect(workspacePerson2.FirstName).toEqual('Jos');
    });
  });
});
