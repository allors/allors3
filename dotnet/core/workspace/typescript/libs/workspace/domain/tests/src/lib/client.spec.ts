import { Client } from '@allors/workspace/domain/json/system';
import { Fixture } from './Fixture';

describe('Client', () => {
  let fixture: Fixture;
  let client: Client;

  beforeEach(async () => {
    fixture = new Fixture();
    await fixture.init();
    client = fixture.client;
  });

  describe('login info', () => {
    it('should be present', async () => {
      expect(client.userId).toBeDefined();
      expect(client.jwtToken).toBeDefined();
    });
  });
});
