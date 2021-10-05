import { MetaPopulation } from '@allors/workspace/meta/system';
import { LazyMetaPopulation } from '@allors/workspace/meta/json/system';
import { data } from '@allors/workspace/meta/json/default';
import { ruleBuilder } from '@allors/workspace/derivations/core-custom';
import { DatabaseClient, DatabaseConnection } from '@allors/workspace/adapters/json/system';
import { Configuration, Engine, PrototypeObjectFactory } from '@allors/workspace/adapters/system';
import { M } from '@allors/workspace/meta/default';

import { FetchClient } from './fetch-client';
import { ISession, IWorkspace, Pull } from '@allors/workspace/domain/system';
import { C1, C2 } from '@allors/workspace/domain/default';

const BASE_URL = 'http://localhost:5000/allors/';
const AUTH_URL = 'TestAuthentication/Token';

export const name_c1A = 'c1A';
export const name_c1B = 'c1B';
export const name_c1C = 'c1C';
export const name_c1D = 'c1D';
export const name_c2A = 'c2A';
export const name_c2B = 'c2B';
export const name_c2C = 'c2C';
export const name_c2D = 'c2D';

export class Fixture {
  jsonClient: FetchClient;
  client: DatabaseClient;
  metaPopulation: MetaPopulation;
  databaseConnection: DatabaseConnection;
  m: M;
  workspace: IWorkspace;

  createDatabaseConnection(): DatabaseConnection {
    const metaPopulation = this.metaPopulation;
    this.m = metaPopulation as unknown as M;

    let nextId = -1;

    const configuration: Configuration = {
      name: 'Default',
      metaPopulation,
      objectFactory: new PrototypeObjectFactory(metaPopulation),
      idGenerator: () => nextId--,
      engine: new Engine(ruleBuilder(this.m)),
    };

    return new DatabaseConnection(configuration);
  }

  async init(population?: string) {
    this.jsonClient = new FetchClient(BASE_URL, AUTH_URL);
    this.client = new DatabaseClient(this.jsonClient);

    await this.jsonClient.setup(population);
    await this.jsonClient.login('jane@example.com', '');

    this.metaPopulation = new LazyMetaPopulation(data);

    this.databaseConnection = this.createDatabaseConnection();
    this.workspace = this.databaseConnection.createWorkspace();
  }

  async pullC1(session: ISession, name: string): Promise<C1> {
    const { client, m } = this;

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C1,
        predicate: {
          kind: 'Equals',
          propertyType: m.C1.Name,
          value: name,
        },
      },
    };

    const result = await client.pull(session, [pull]);
    return result.collection<C1>(m.C1)[0];
  }

  async pullC2(session: ISession, name: string): Promise<C2> {
    const { client, m } = this;

    const pull: Pull = {
      extent: {
        kind: 'Filter',
        objectType: m.C2,
        predicate: {
          kind: 'Equals',
          propertyType: m.C2.Name,
          value: name,
        },
      },
    };

    const result = await client.pull(session, [pull]);
    return result.collection<C2>(m.C2)[0];
  }

  async login(login: string, password?: string): Promise<boolean> {
    return this.jsonClient.login(login, password);
  }
}
