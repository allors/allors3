import { IAsyncDatabaseClient, IWorkspace, IReactiveDatabaseClient } from '@allors/workspace/domain/system';
import '@allors/workspace/domain/default';
import { C1 } from '@allors/workspace/domain/default';

import { Fixture } from '../../Fixture';
import '../../Matchers';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initDatabaseManyToMany(
  asyncClient: IAsyncDatabaseClient,
  reactiveClient: IReactiveDatabaseClient,
  workspace: IWorkspace,
  login: (login: string) => Promise<boolean>,
  createWorkspace: () => IWorkspace,
  createExclusiveWorkspace: () => IWorkspace
) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login, createWorkspace, createExclusiveWorkspace);
}

export async function databaseManyToManySetRole() {
  /* Single Session
   */
  //#region No push before add
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pullAsync(session, { object: c1a });

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }

  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pullAsync(session, { object: c1b });

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }

  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pullAsync(session, [{ object: c1a }, { object: c1b }]);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  //#region Push c1a to database before add
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);

    await client.pushAsync(session);

    const c1b = session.create<C1>(m.C1);

    expect(c1a.CanWriteC1C1Many2Manies).toBeFalsy();
    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);
  }
  //#endregion

  //#region Push/Pull c1a to database before add
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);

    await client.pushAsync(session);
    await client.pullAsync(session, { object: c1a });

    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  //#region Push c1b to database before add
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1b = session.create<C1>(m.C1);

    await client.pushAsync(session);

    const c1a = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  //#region Push c1a and c1b to database before add
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    await client.pushAsync(session);

    expect(c1a.CanWriteC1C1Many2Manies).toBeFalsy();
    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);
  }
  //#endregion

  //#region Push/Pull c1a and c1b to database before add
  {
    const { m, client, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    await client.pushAsync(session);
    await client.pullAsync(session, [{ object: c1a }, { object: c1a }]);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await client.pushAsync(session);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  /* Multiple Sessions
   */
  //#region c1a in other session
  {
    const { m, client, workspace } = fixture;
    const session1 = workspace.createSession();
    const session2 = workspace.createSession();

    const c1a_2 = session2.create<C1>(m.C1);
    const c1b_1 = session1.create<C1>(m.C1);

    await client.pushAsync(session2);
    await client.pullAsync(session1, { object: c1a_2 });

    const c1a_1 = session1.instantiate(c1a_2);

    c1a_1.addC1C1Many2Many(c1b_1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);

    await client.pushAsync(session1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);
  }
  //#endregion
  //#region c1b in other session
  {
    const { m, client, workspace } = fixture;
    const session1 = workspace.createSession();
    const session2 = workspace.createSession();

    const c1a_1 = session1.create<C1>(m.C1);
    const c1b_2 = session2.create<C1>(m.C1);

    await client.pushAsync(session2);
    await client.pullAsync(session1, { object: c1b_2 });

    const c1b_1 = session1.instantiate(c1b_2);

    c1a_1.addC1C1Many2Many(c1b_1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);

    await client.pushAsync(session1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);
  }
  //#endregion
  //#region c1a and c1b in other session
  {
    const { m, client, workspace } = fixture;
    const session1 = workspace.createSession();
    const session2 = workspace.createSession();

    const c1a_2 = session2.create<C1>(m.C1);
    const c1b_2 = session2.create<C1>(m.C1);

    await client.pushAsync(session2);
    await client.pullAsync(session1, [{ object: c1a_2 }, { object: c1b_2 }]);

    const c1a_1 = session1.instantiate(c1a_2);
    const c1b_1 = session1.instantiate(c1b_2);

    c1a_1.addC1C1Many2Many(c1b_1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);

    await client.pushAsync(session1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);
  }
  //#endregion
}
