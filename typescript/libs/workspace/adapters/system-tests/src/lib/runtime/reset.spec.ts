import { IAsyncDatabaseClient, IReactiveDatabaseClient, IWorkspace } from '@allors/workspace/domain/system';
import { Fixture, name_c1A, name_c1B } from '../Fixture';
import '../Matchers';
import '@allors/workspace/domain/core';

let fixture: Fixture;

it('dummy', () => {
  // otherwise jest will complain that there are no specs
  expect(true).toBeTruthy();
});

export async function initReset(asyncClient: IAsyncDatabaseClient, reactiveClient: IReactiveDatabaseClient, workspace: IWorkspace, login: (login: string) => Promise<boolean>) {
  fixture = new Fixture(asyncClient, reactiveClient, workspace, login);
}

export async function resetUnitWithoutPush() {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  c1a.strategy.reset();

  expect(c1a.C1AllorsString).toBeNull();
}

export async function resetUnitAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1AllorsString).toBeNull();
}

export async function resetUnitAfterDoublePush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  c1a.C1AllorsString = 'X';

  await client.pushAsync(session);
  await client.pullAsync(session, { object: c1a });

  c1a.C1AllorsString = 'Y';

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1AllorsString).toBe('X');
}

export async function resetOne2OneWithoutPush() {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1One2One = c1b;

  c1a.strategy.reset();

  expect(c1a.C1C1One2One).toBeNull();
}

export async function resetOne2OneAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1One2One = c1b;

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1One2One).toBeNull();
}

export async function resetOne2OneRemoveAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1One2One = c1b;

  await client.pushAsync(session);
  await client.pullAsync(session, { object: c1a });

  c1a.C1C1One2One = null;

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1One2One).toBe(c1b);
}

export async function resetMany2OneWithoutPush() {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1Many2One = c1b;

  c1a.strategy.reset();

  expect(c1a.C1C1Many2One).toBeNull();
}

export async function resetMany2OneAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1Many2One = c1b;

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1Many2One).toBeNull();
}

export async function resetMany2OneRemoveAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.C1C1Many2One = c1b;

  await client.pushAsync(session);
  await client.pullAsync(session, { object: c1a });

  c1a.C1C1Many2One = null;

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1Many2One).toBe(c1b);
}

export async function resetOne2ManyWithoutPush() {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1One2Many(c1b);

  c1a.strategy.reset();

  expect(c1a.C1C1One2Manies.length).toBe(0);
}

export async function resetOne2ManyAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1One2Many(c1b);

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1One2Manies.length).toBe(0);
}

export async function resetOne2ManyRemoveAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1One2Many(c1b);

  await client.pushAsync(session);
  await client.pullAsync(session, { object: c1a });

  c1a.removeC1C1One2Many(c1b);

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1One2Manies.length).toBe(1);
  expect(c1a.C1C1One2Manies).toContain(c1b);
}

export async function resetMany2ManyWithoutPush() {
  const { workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1Many2Many(c1b);

  c1a.strategy.reset();

  expect(c1a.C1C1Many2Manies.length).toBe(0);
}

export async function resetMany2ManyAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1Many2Many(c1b);

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1Many2Manies.length).toBe(0);
}

export async function resetMany2ManyRemoveAfterPush() {
  const { client, workspace } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c1b = await fixture.pullC1(session, name_c1B);

  c1a.addC1C1Many2Many(c1b);

  await client.pushAsync(session);
  await client.pullAsync(session, { object: c1a });

  c1a.removeC1C1Many2Many(c1b);

  await client.pushAsync(session);

  c1a.strategy.reset();

  expect(c1a.C1C1Many2Manies.length).toBe(1);
  expect(c1a.C1C1Many2Manies).toContain(c1b);
}
