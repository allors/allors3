import { Client } from '@allors/workspace/domain/json/rxjs/system';
import { Fixture } from './Fixture';

describe('Client', () => {
  let fixture: Fixture;
  let client: Client;
  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();
    client = fixture.client;
  });

  describe('login', () => {
    beforeEach(async () => {
      await fixture.login();
    });

    it('userId and jwtToken should be present', async () => {
      expect(client.userId).toBeDefined();
      expect(client.jwtToken).toBeDefined();
    });
  });
});
