import { MetaPopulation } from '@allors/workspace/meta/system';
import { FetchClient } from './FetchClient';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/core';
import { Database } from '@allors/workspace/adapters/json/system';
import { Configuration, ObjectFactory } from '@allors/workspace/adapters/system';
import { WorkspaceServices } from '@allors/workspace/adapters/system-tests';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export class Fixture {
  client: FetchClient;
  metaPopulation: MetaPopulation;
  database: Database;

  async init(population?: string) {
    this.client = new FetchClient(BASE_URL, AUTH_URL);

    await this.client.setup(population);
    await this.client.login('administrator', '');

    this.metaPopulation = new LazyMetaPopulation(data);

    let nextId = -1;
    this.database = new Database(
      new Configuration('Default', this.metaPopulation, new ObjectFactory(this.metaPopulation)),
      () => {
        return new WorkspaceServices();
      },
      () => nextId--,
      this.client
    );
  }
}
