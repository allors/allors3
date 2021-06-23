import { PullRequest, ExtentKind } from '@allors/protocol/json/system';
import { Database } from '@allors/workspace/adapters/json/system';
import { M } from '@allors/workspace/meta/json/core';
import { Fixture } from '../Fixture';
import { WorkspaceServices } from '../WorkspaceServices';
import { Configuration, ObjectFactory } from '@workspace/adapters/json/system';

describe('Database', () => {
  let fixture: Fixture;
  let m: M;
  let database: Database;

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();
    m = fixture.m;

    let nextId = -1;
    database = new Database(
      new Configuration('Default', fixture.metaPopulation, new ObjectFactory()),
      fixture.client,
      () => {
        return new WorkspaceServices();
      },
      () => nextId--
    );
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
