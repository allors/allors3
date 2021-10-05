import { Fixture } from '../../../fixture';
import '../../../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('sessionUnit', async () => {
  const { client, workspace, m } = fixture;
  const session = workspace.createSession();

  expect(true).toBeTruthy();
});
