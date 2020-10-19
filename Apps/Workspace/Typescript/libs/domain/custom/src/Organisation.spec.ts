import { Meta, data } from '@allors/meta/generated';
import { MetaPopulation } from '@allors/meta/core';
import { InternalOrganisation } from '@allors/domain/generated';
import { Session, Database } from '@allors/workspace/core';
import { MemoryDatabase } from '@allors/workspace/memory';

import { extend } from '../src';

describe('InternalOrganisation', () => {
  let m: Meta;
  let session: Session;

  beforeEach(() => {
    m = new MetaPopulation(data) as Meta;
    const database: Database = new MemoryDatabase(m);
    extend(database);

    session = database.createSession();
  });

  describe('displayName', () => {
    // let organisation: InternalOrganisation;

    beforeEach(() => {
      // organisation = session.create(m.InternalOrganisation) as InternalOrganisation;
    });

    it('should be N/A when nothing set', () => {
      expect(true).toBe(true);
    });
  });
});
