import { Workspace } from '@allors/workspace/adapters/json/system';
import { M } from '@allors/workspace/meta/core';
import { Fixture } from '../Fixture';
import { WorkspaceServices } from '../WorkspaceServices';

describe('Workspace', () => {
  let fixture: Fixture;
  let m: M;
  let workspace: Workspace;

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();
    m = fixture.m;
    workspace = new Workspace('Default', fixture.metaPopulation, new WorkspaceServices(), fixture.client);
  });

  it('pull', async () => {
    expect(workspace).toBeDefined();
  });
});
