import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { ReactiveDatabaseClient, DatabaseConnection } from '@allors/workspace/adapters/json/system';
import { Configuration, Engine, PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { ruleBuilder } from '@allors/workspace/derivations/core-custom';
import { M } from '@allors/workspace/meta/default';

import { RxjsClient } from './rxjs-client';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export class Fixture {
  client: RxjsClient;
  reactiveDatabaseClient: ReactiveDatabaseClient;
  metaPopulation: MetaPopulation;
  databaseConnection: DatabaseConnection;

  createDatabaseConnection(): DatabaseConnection {
    const metaPopulation = this.metaPopulation;
    const m = metaPopulation as unknown as M;

    let nextId = -1;

    const configuration: Configuration = {
      name: 'Default',
      metaPopulation,
      objectFactory: new PrototypeObjectFactory(metaPopulation),
      idGenerator: () => nextId--,
      engine: new Engine(ruleBuilder(m)),
    };

    return new DatabaseConnection(configuration);
  }

  async init(population?: string) {
    this.client = new RxjsClient(BASE_URL, AUTH_URL);
    this.reactiveDatabaseClient = new ReactiveDatabaseClient(this.client);

    await this.client.setup(population);
    await this.client.login('jane@example.com', '');

    this.metaPopulation = new LazyMetaPopulation(data);
    this.databaseConnection = this.createDatabaseConnection();
  }
}
