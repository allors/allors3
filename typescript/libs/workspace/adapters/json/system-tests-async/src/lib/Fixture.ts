import { MetaPopulation } from '@allors/workspace/meta/system';
import { FetchClient } from './FetchClient';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/core';
import { ruleBuilder } from '@allors/workspace/domain/core';
import { AsyncDatabaseClient, DatabaseConnection } from '@allors/workspace/adapters/json/system';
import { Configuration, PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { WorkspaceServices } from '@allors/workspace/adapters/system-tests';
import { M } from '@allors/workspace/meta/core';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export class Fixture {
  client: FetchClient;
  asyncDatabaseClient: AsyncDatabaseClient;
  metaPopulation: MetaPopulation;
  databaseConnection: DatabaseConnection;

  createDatabaseConnection(): DatabaseConnection {
    let nextId = -1;
    return new DatabaseConnection(
      new Configuration('Default', this.metaPopulation, new PrototypeObjectFactory(this.metaPopulation)),
      () => nextId--,
      () => {
        return new WorkspaceServices(ruleBuilder(this.metaPopulation as M));
      }
    );
  }

  async init(population?: string) {
    this.client = new FetchClient(BASE_URL, AUTH_URL);
    this.asyncDatabaseClient = new AsyncDatabaseClient(this.client);

    await this.client.setup(population);
    await this.client.login('administrator', '');

    this.metaPopulation = new LazyMetaPopulation(data);

    this.databaseConnection = this.createDatabaseConnection();
  }
}
