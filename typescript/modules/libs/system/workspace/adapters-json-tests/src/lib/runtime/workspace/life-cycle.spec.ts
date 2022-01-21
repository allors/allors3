import { WC1 } from '@allors/default/workspace/domain';
import { Fixture } from '../../fixture';
import '../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('workspaceInstantiateOtherSession', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<WC1>(m.WC1);

  let objectSession2 = session2.instantiate(objectSession1);

  expect(objectSession2).toBeNull();

  session1.pushToWorkspace();

  objectSession2 = session2.instantiate(objectSession1);

  expect(objectSession2).not.toBeNull();
});

test('workspacePullOtherSessionShouldThrowError', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<WC1>(m.WC1);

  let hasErrors = false;
  try {
    await session2.pull({ object: objectSession1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('workspaceCrossSession', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<WC1>(m.WC1);
  const objectSession2 = session2.create<WC1>(m.WC1);

  let hasErrors = false;
  try {
    objectSession1.addWorkspaceWC1Many2Many(objectSession2);
  } catch (error) {
    hasErrors = true;
    expect(error.message).toBe('Strategy is from a different session');
  }

  expect(hasErrors).toBeTruthy();
});
