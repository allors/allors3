import { SC1 } from '@allors/workspace/domain/default';
import { Fixture } from '../../fixture';
import '../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('sessionInstantiateOtherSession', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<SC1>(m.SC1);

  const objectSession2 = session2.instantiate(objectSession1);

  expect(objectSession2).toBeNull();
});

test('sessionPullOtherSessionShouldThrowError', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<SC1>(m.SC1);

  let hasErrors = false;
  try {
    await session2.pull({ object: objectSession1 });
  } catch {
    hasErrors = true;
  }

  expect(hasErrors).toBeTruthy();
});

test('sessionCrossSessionShouldThrowError', async () => {
  const { workspace, m } = fixture;
  const session1 = workspace.createSession();
  const session2 = workspace.createSession();

  const objectSession1 = session1.create<SC1>(m.SC1);
  const objectSession2 = session2.create<SC1>(m.SC1);

  let hasErrors = false;
  try {
    objectSession1.addSessionSC1Many2Many(objectSession2);
  } catch (error) {
    hasErrors = true;
    expect(error.message).toBe('Strategy is from a different session');
  }

  expect(hasErrors).toBeTruthy();
});
