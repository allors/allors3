import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { ruleBuilder } from '@allors/core/workspace/derivations-custom';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';
import { M } from '@allors/default/workspace/meta';

import { FetchClient } from './fetch-client';
import {
  Configuration,
  ISession,
  IWorkspace,
  Pull,
} from '@allors/system/workspace/domain';
import { C1, C2 } from '@allors/default/workspace/domain';

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
      rules: ruleBuilder(this.m),
    };

    return new DatabaseConnection(configuration, this.jsonClient);
  }

  createExclusiveWorkspace(): IWorkspace {
    return this.databaseConnection.createWorkspace();
  }
  createWorkspace(): IWorkspace {
    return this.createDatabaseConnection().createWorkspace();
  }

  async init(population?: string) {
    this.jsonClient = new FetchClient(BASE_URL, AUTH_URL);

    await this.jsonClient.setup(population);
    await this.jsonClient.login('jane@example.com', '');

    this.metaPopulation = new LazyMetaPopulation(data);

    this.databaseConnection = this.createDatabaseConnection();
    this.workspace = this.databaseConnection.createWorkspace();
  }

  async pullC1(session: ISession, name: string): Promise<C1> {
    const { m } = this;

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

    const result = await session.pull([pull]);
    return result.collection<C1>(m.C1)[0];
  }

  async pullC2(session: ISession, name: string): Promise<C2> {
    const { m } = this;

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

    const result = await session.pull([pull]);
    return result.collection<C2>(m.C2)[0];
  }

  async login(login: string, password?: string): Promise<boolean> {
    return this.jsonClient.login(login, password);
  }
}
