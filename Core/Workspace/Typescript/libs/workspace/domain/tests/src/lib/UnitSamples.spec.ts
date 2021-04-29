import { Fixture } from './Fixture';

describe('Unit Samples', () => {
  let fixture: Fixture;

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();
  });

  describe('Step 1', () => {
    it('should return units with values', async () => {
      console.log(`ok`)
    });
  });
});
