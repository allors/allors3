import { PullTests } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;
let tests: PullTests;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  tests = new PullTests(fixture.database, (login) => fixture.client.login(login));
});

test('andGreaterThanLessThan', async () => {
  await tests.andGreaterThanLessThan();
});
