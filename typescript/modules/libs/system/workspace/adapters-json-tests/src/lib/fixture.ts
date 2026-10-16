import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { ruleBuilder } from '@allors/core/workspace/derivations-test';
import { DatabaseConnection } from '@allors/system/workspace/adapters-json';
import { PrototypeObjectFactory } from '@allors/system/workspace/adapters';
import { M } from '@allors/default/workspace/meta';

import { FetchClient, OwnedTransport } from './fetch-client';
import {
  Configuration,
  ISession,
  IWorkspace,
  Pull,
} from '@allors/system/workspace/domain';
import { C1, C2 } from '@allors/default/workspace/domain';
import { DatabaseConnectionOptions } from '@allors/system/workspace/adapters-json';

const BASE_URL = 'http://localhost:5000/allors/';

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

  private resources = new Map<
    DatabaseConnection,
    { transport: OwnedTransport; managed: boolean; closing?: Promise<void> }
  >();

  createConfiguration(name = 'Default'): Configuration {
    const metaPopulation = this.metaPopulation;
    this.m = metaPopulation as unknown as M;
    let nextId = -1;
    return {
      name,
      metaPopulation,
      objectFactory: new PrototypeObjectFactory(metaPopulation),
      idGenerator: () => nextId--,
      rules: ruleBuilder(this.m),
    };
  }

  async createConnection(
    options: DatabaseConnectionOptions = { revision: 'core-read-v1' },
    transport: OwnedTransport = this.jsonClient.createTransport(),
    managed = false
  ): Promise<DatabaseConnection> {
    try {
      const connection = await DatabaseConnection.create(
        this.createConfiguration(),
        transport,
        options
      );
      this.resources.set(connection, { transport, managed });
      return connection;
    } catch (error) {
      transport.dispose();
      throw error;
    }
  }

  closeConnection(connection: DatabaseConnection): Promise<void> {
    const resource = this.resources.get(connection);
    if (!resource) return connection.close();
    if (!resource.closing)
      resource.closing = connection.close().finally(() => {
        try {
          resource.transport.dispose();
        } finally {
          this.resources.delete(connection);
        }
      });
    return resource.closing;
  }

  private async closeResources(managedOnly = false): Promise<void> {
    const outcomes = await Promise.allSettled(
      [...this.resources]
        .filter(([, resource]) => !managedOnly || resource.managed)
        .map(([connection]) => this.closeConnection(connection))
    );
    const failure = outcomes.find(
      (outcome) => outcome.status === 'rejected'
    ) as PromiseRejectedResult | undefined;
    if (failure) throw failure.reason;
  }

  close(): Promise<void> {
    return this.closeResources();
  }

  createExclusiveWorkspace(): IWorkspace {
    return this.databaseConnection.createWorkspace();
  }

  async createWorkspace(): Promise<IWorkspace> {
    return (
      await this.createConnection(undefined, undefined, true)
    ).createWorkspace();
  }

  async init(population?: string) {
    this.jsonClient = new FetchClient(process.env.ALLORS_TEST_URL ?? BASE_URL);
    this.metaPopulation = new LazyMetaPopulation(data);
    await this.jsonClient.setup(population);
    await this.login('jane@example.com', '');
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
    await this.closeResources(true);
    this.databaseConnection = undefined;
    this.workspace = undefined;
    const authenticated = await this.jsonClient.login(login, password);
    if (!authenticated) return false;
    this.databaseConnection = await this.createConnection(
      undefined,
      undefined,
      true
    );
    this.workspace = this.databaseConnection.createWorkspace();
    return true;
  }
}
