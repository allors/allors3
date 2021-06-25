import { M } from '@allors/workspace/meta/core';
import { Configuration, ObjectFactory } from '@allors/workspace/adapters/system';
import { Database } from '@allors/workspace/adapters/json/system';
import { PullRequest, ExtentKind } from '@allors/protocol/json/system';
import { Fixture } from '../Fixture';
import { WorkspaceServices } from '../WorkspaceServices';

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
      new Configuration('Default', fixture.metaPopulation, new ObjectFactory(fixture.metaPopulation)),
      () => {
        return new WorkspaceServices();
      },
      () => nextId--,
      fixture.client
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

    const pullResponse = await database.pull(pullRequest);

    expect(pullResponse).toBeDefined();
  });
});
