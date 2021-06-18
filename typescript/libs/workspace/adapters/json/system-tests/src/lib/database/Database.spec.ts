import { ExtentKind, PullRequest } from '@allors/protocol/json/system';
import { Database, Identities } from '@allors/workspace/adapters/json/system';
import { M } from '@allors/workspace/meta/core';
import { Fixture } from '../Fixture';

describe('Database', () => {
  let fixture: Fixture;
  let m: M;
  let database: Database;

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();
    m = fixture.m;
    database = new Database(m, fixture.client, new Identities());
  });

  it('call pull', async () => {
    const pullRequest: PullRequest = {
      l: [
        {
          e: {
            k: ExtentKind.Extent,
            t: m.C1.tag,
          },
        },
      ],
    };

    const pullResponse = await database.pull(pullRequest).toPromise();

    expect(pullResponse).toBeDefined();
  });
});
