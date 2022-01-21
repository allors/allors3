import { C1 } from '@allors/default/workspace/domain';
import { Fixture } from '../fixture';
import '../matchers';

let fixture: Fixture;

beforeEach(async () => {
  fixture = new Fixture();
  await fixture.init();
});

test('sandbox', async () => {
  const { workspace, m } = fixture;
  const session = workspace.createSession();

  const c1x = session.create<C1>(m.C1);
  const c1y_2 = session.create<C1>(m.C1);

  await session.push();
  const result = await session.pull({ object: c1y_2 });
  const c1y_1 = result.objects.values().next().value;

  if (!c1x.canWriteC1C1Many2Manies) {
    await session.pull({ object: c1x });
  }

  c1x.addC1C1Many2Many(c1y_1);

  expect(c1x.C1C1Many2Manies.length).toBe(1);
  expect(c1x.C1C1Many2Manies).toContain(c1y_1);

  expect(c1y_1.C1sWhereC1C1Many2Many.length).toBe(1);
  expect(c1y_1.C1sWhereC1C1Many2Many).toContain(c1x);
});
