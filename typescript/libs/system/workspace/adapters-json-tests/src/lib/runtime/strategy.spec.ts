import { Fixture, name_c1A, name_c2A } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('setUnitWrongObjectType', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);

  let hasErrors: boolean;

  try {
    c1a.strategy.setUnitRole(m.C1.C1AllorsInteger, 'Not an integer');
    hasErrors = false;
  } catch (e) {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('setCompositeRoleWrongObjectType', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c2a = await fixture.pullC2(session, name_c2A);

  let hasErrors: boolean;

  try {
    c1a.strategy.setCompositeRole(m.C1.C1C1One2One, c2a);
    hasErrors = false;
  } catch (e) {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('setCompositeRoleWrongRoleType', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c2a = await fixture.pullC2(session, name_c2A);

  let hasErrors: boolean;

  try {
    c1a.strategy.setCompositeRole(m.C1.C1C2Many2Manies, c2a);
    hasErrors = false;
  } catch (e) {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('addCompositesWrongObjectType', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c2a = await fixture.pullC2(session, name_c2A);

  let hasErrors: boolean;

  try {
    c1a.strategy.addCompositesRole(m.C1.C1C1Many2Manies, c2a);
    hasErrors = false;
  } catch (e) {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('addCompositesRoleWrongRoleType', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1a = await fixture.pullC1(session, name_c1A);
  const c2a = await fixture.pullC2(session, name_c2A);

  let hasErrors: boolean;

  try {
    c1a.strategy.addCompositesRole(m.C1.C1C2One2One, c2a);
    hasErrors = false;
  } catch (e) {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});
