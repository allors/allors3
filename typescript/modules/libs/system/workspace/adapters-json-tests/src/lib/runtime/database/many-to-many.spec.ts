import { C1 } from '@allors/default/workspace/domain';
import { Fixture } from '../../fixture';
import '../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('databaseManyToManySetRole', async () => {
  /* Single Session
   */
  //#region No push before add
  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.pull({ object: c1a });

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }

  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.pull({ object: c1b });

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }

  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.pull([{ object: c1a }, { object: c1b }]);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  //#region Push c1a to database before add
  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);

    await session.push();

    const c1b = session.create<C1>(m.C1);

    expect(c1a.canWriteC1C1Many2Manies).toBeFalsy();
    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);
  }
  //#endregion

  //#region Push/Pull c1a to database before add
  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);

    await session.push();
    await session.pull({ object: c1a });

    const c1b = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  //#region Push c1b to database before add
  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1b = session.create<C1>(m.C1);

    await session.push();

    const c1a = session.create<C1>(m.C1);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  //#region Push c1a and c1b to database before add
  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    await session.push();

    expect(c1a.canWriteC1C1Many2Manies).toBeFalsy();
    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([]);
  }
  //#endregion

  //#region Push/Pull c1a and c1b to database before add
  {
    const { m, workspace } = fixture;
    const session = workspace.createSession();

    const c1a = session.create<C1>(m.C1);
    const c1b = session.create<C1>(m.C1);

    await session.push();
    await session.pull([{ object: c1a }, { object: c1a }]);

    c1a.addC1C1Many2Many(c1b);

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);

    await session.push();

    expect(c1a.C1C1Many2Manies).toEqual([c1b]);
    expect(c1b.C1sWhereC1C1Many2Many).toEqual([c1a]);
  }
  //#endregion

  /* Multiple Sessions
   */
  //#region c1a in other session
  {
    const { m, workspace } = fixture;
    const session1 = workspace.createSession();
    const session2 = workspace.createSession();

    const c1a_2 = session2.create<C1>(m.C1);
    const c1b_1 = session1.create<C1>(m.C1);

    await session2.push();
    await session1.pull({ object: c1a_2 });

    const c1a_1 = session1.instantiate(c1a_2);

    c1a_1.addC1C1Many2Many(c1b_1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);

    await session1.push();

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);
  }
  //#endregion
  //#region c1b in other session
  {
    const { m, workspace } = fixture;
    const session1 = workspace.createSession();
    const session2 = workspace.createSession();

    const c1a_1 = session1.create<C1>(m.C1);
    const c1b_2 = session2.create<C1>(m.C1);

    await session2.push();
    await session1.pull({ object: c1b_2 });

    const c1b_1 = session1.instantiate(c1b_2);

    c1a_1.addC1C1Many2Many(c1b_1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);

    await session1.push();

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);
  }
  //#endregion
  //#region c1a and c1b in other session
  {
    const { m, workspace } = fixture;
    const session1 = workspace.createSession();
    const session2 = workspace.createSession();

    const c1a_2 = session2.create<C1>(m.C1);
    const c1b_2 = session2.create<C1>(m.C1);

    await session2.push();
    await session1.pull([{ object: c1a_2 }, { object: c1b_2 }]);

    const c1a_1 = session1.instantiate(c1a_2);
    const c1b_1 = session1.instantiate(c1b_2);

    c1a_1.addC1C1Many2Many(c1b_1);

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);

    await session1.push();

    expect(c1a_1.C1C1Many2Manies).toEqual([c1b_1]);
    expect(c1b_1.C1sWhereC1C1Many2Many).toEqual([c1a_1]);
  }
  //#endregion
});
