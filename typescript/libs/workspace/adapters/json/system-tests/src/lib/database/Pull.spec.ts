import { init, andGreaterThanLessThan } from '@allors/workspace/adapters/system-tests';
import { Fixture } from '../Fixture';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
  await init(fixture.database, (login) => fixture.client.login(login));
});

test('andGreaterThanLessThan', async () => {
  await andGreaterThanLessThan();
});
