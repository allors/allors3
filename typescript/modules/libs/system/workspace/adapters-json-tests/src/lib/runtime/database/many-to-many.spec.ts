import { C1 } from '@allors/default/workspace/domain';
import { ICompositesDiff } from '@allors/system/workspace/domain';
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

test('databaseManyToManyAddAndRemoveWithoutPullingMembers', async () => {
  const { m, workspace } = fixture;
  const roleType = m.C1.C1C1Many2Manies;
  const associationType = roleType.associationType;
  const sorted = (ids: number[]) => [...ids].sort((a, b) => a - b);

  const session1 = workspace.createSession();
  const c1a_1 = session1.create<C1>(m.C1);
  const c1b_1 = session1.create<C1>(m.C1);
  const c1c_1 = session1.create<C1>(m.C1);
  c1a_1.addC1C1Many2Many(c1b_1);
  c1a_1.addC1C1Many2Many(c1c_1);
  await session1.push();

  const session2 = workspace.createSession();
  await session2.pull([{ object: c1a_1 }, { object: c1b_1 }]);
  const c1a_2 = session2.instantiate<C1>(c1a_1.id);
  const c1b_2 = session2.instantiate<C1>(c1b_1.id);
  const c1d_2 = session2.create<C1>(m.C1);
  const c1dWorkspaceId = c1d_2.id;

  expect(c1a_2.strategy.getCompositesRoleIds(roleType)).toEqual(
    sorted([c1b_1.id, c1c_1.id])
  );
  expect(() => c1a_2.C1C1Many2Manies).toThrow(
    'in role C1.C1C1Many2Manies is not in this session'
  );

  c1a_2.addC1C1Many2Many(c1d_2);
  c1a_2.removeC1C1Many2Many(c1b_2);

  expect(c1a_2.strategy.getCompositesRoleIds(roleType)).toEqual(
    sorted([c1c_1.id, c1dWorkspaceId])
  );
  expect(c1a_2.strategy.existRole(roleType)).toBe(true);
  expect(c1d_2.C1sWhereC1C1Many2Many).toEqual([c1a_2]);
  expect(c1b_2.C1sWhereC1C1Many2Many).toEqual([]);

  const diff = c1a_2.strategy
    .diff()
    .find((v) => v.relationType === roleType.relationType) as ICompositesDiff;
  expect([...diff.originalRoleIds]).toEqual(sorted([c1b_1.id, c1c_1.id]));
  expect([...diff.changedRoleIds]).toEqual(sorted([c1c_1.id, c1dWorkspaceId]));
  expect(diff.originalRoles).toEqual([c1b_2]);
  expect(diff.changedRoles).toEqual([c1d_2]);

  const changeSet = session2.checkpoint();
  expect(changeSet.associationsByRoleType.get(roleType)).toEqual(
    new Set([c1a_2])
  );
  expect(changeSet.rolesByAssociationType.get(associationType)).toEqual(
    new Set([c1b_2, c1d_2])
  );
  expect(changeSet.roleIdsByAssociationType.get(associationType)).toEqual(
    new Set([c1b_1.id, c1dWorkspaceId])
  );

  const pushSpy = jest.spyOn(fixture.jsonClient, 'push');
  await session2.push();
  const pushedRole = pushSpy.mock.calls[0][0].o
    .find((v) => v.d === c1a_1.id)
    .r.find((v) => v.t === roleType.relationType.tag);
  pushSpy.mockRestore();
  expect(pushedRole.a).toEqual([c1dWorkspaceId]);
  expect(pushedRole.r).toEqual([c1b_1.id]);

  const session3 = workspace.createSession();
  await session3.pull({
    object: c1a_1,
    results: [{ include: [{ propertyType: roleType }] }],
  });
  const c1a_3 = session3.instantiate<C1>(c1a_1.id);
  expect(sorted(c1a_3.C1C1Many2Manies.map((v) => v.id))).toEqual(
    sorted([c1c_1.id, c1d_2.id])
  );
});
