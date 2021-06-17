import { AjaxClient, Database, Identities } from '@allors/workspace/adapters/json/system';
import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/lazy/system';
import { M, data } from '@allors/workspace/meta/core';
import { ajax } from 'rxjs/ajax';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export class Fixture {
  metaPopulation: MetaPopulation;
  m: M;
  database: Database;

  async init(population = 'full') {
    await ajax(`${BASE_URL}Test/Setup?population=${population}`).toPromise();

    const client = new AjaxClient(BASE_URL, AUTH_URL);
    await client.login('administrator', '').toPromise();

    this.metaPopulation = new LazyMetaPopulation(data);
    this.m = (this.metaPopulation as MetaPopulation) as M;
    const identities = new Identities();

    this.database = new Database(this.m, client, identities);
  }
}
