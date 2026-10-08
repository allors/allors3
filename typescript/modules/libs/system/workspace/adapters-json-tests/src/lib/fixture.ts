import { MetaPopulation } from '@allors/system/workspace/meta';
import { LazyMetaPopulation } from '@allors/system/workspace/meta-json';
import { data } from '@allors/default/workspace/meta-json';
import { ruleBuilder } from '@allors/core/workspace/derivations-test';
import {
  DatabaseConnection,
  DatabaseConnectionOptions,
} from '@allors/system/workspace/connection';
import { PrototypeObjectFactory, Workspace } from '@allors/system/workspace/session';
import { M } from '@allors/default/workspace/meta';

import { FetchTransport } from './fetch-transport';
import {
  IObject,
  IObjectFactory,
  IRule,
  ISession,
  IWorkspace,
  Operations,
  Pull,
} from '@allors/system/workspace/domain';
import { RoleType } from '@allors/system/workspace/meta';
import { C1, C2 } from '@allors/default/workspace/domain';

const BASE_URL = 'http://localhost:5000/allors/';

const WORKSPACE_NAME = 'Default';

export const name_c1A = 'c1A';
export const name_c1B = 'c1B';
export const name_c1C = 'c1C';
export const name_c1D = 'c1D';
export const name_c2A = 'c2A';
export const name_c2B = 'c2B';
export const name_c2C = 'c2C';
export const name_c2D = 'c2D';

/**
 * The harness of the tests against the Core test server: one meta population, object factory
 * and rule set; a connection and a workspace for the signed-in user, replaced by login, as a
 * connection serves one user; and a transport of its own for every connection a test builds.
 */
export class Fixture {
  metaPopulation: MetaPopulation;
  m: M;
  objectFactory: IObjectFactory;
  rules: IRule<IObject>[];
  userName: string;
  transport: FetchTransport;
  connection: DatabaseConnection;
  workspace: IWorkspace;

  /**
   * A transport of its own, authenticated as the user, for a connection the test builds itself.
   */
  createTransport(userName = this.userName): FetchTransport {
    const transport = new FetchTransport(BASE_URL);
    transport.login(userName);
    return transport;
  }

  createConnection(
    userName = this.userName,
    options?: DatabaseConnectionOptions
  ): DatabaseConnection {
    return new DatabaseConnection(
      WORKSPACE_NAME,
      this.metaPopulation,
      this.createTransport(userName),
      options
    );
  }

  createWorkspaceOn(connection: DatabaseConnection): IWorkspace {
    return new Workspace(connection, this.objectFactory, this.rules);
  }

  createExclusiveWorkspace(): IWorkspace {
    return this.createWorkspaceOn(this.connection);
  }

  createWorkspace(): IWorkspace {
    return this.createWorkspaceOn(this.createConnection());
  }

  async init(population?: string) {
    this.transport = new FetchTransport(BASE_URL);
    await this.transport.setup(population);

    this.metaPopulation = new LazyMetaPopulation(data);
    this.m = this.metaPopulation as unknown as M;
    this.objectFactory = new PrototypeObjectFactory(this.metaPopulation);
    this.rules = ruleBuilder(this.m);

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

  /**
   * Takes the permission for the operation on the role type away from the Administrator
   * role, in the database, so that the grant of the administrators changes version.
   */
  async removeAdministratorPermission(
    roleType: RoleType,
    operation: Operations
  ): Promise<void> {
    await this.transport.get(
      `Test/RemoveAdministratorPermission?relationType=${encodeURIComponent(
        roleType.relationType.tag
      )}&operation=${Operations[operation]}`
    );
  }

  /**
   * Adds the permission for the operation on the role type to the revocation that the
   * Denied objects carry, in the database, so that the revocation changes version.
   */
  async denyPermission(roleType: RoleType, operation: Operations): Promise<void> {
    await this.transport.get(
      `Test/DenyPermission?relationType=${encodeURIComponent(
        roleType.relationType.tag
      )}&operation=${Operations[operation]}`
    );
  }

  /**
   * Signs in as the user: a connection of its own, as a connection serves one user, and a
   * workspace on it.
   */
  async login(login: string, password?: string): Promise<boolean> {
    this.userName = login;
    this.transport = this.createTransport(login);
    this.connection = new DatabaseConnection(
      WORKSPACE_NAME,
      this.metaPopulation,
      this.transport
    );
    this.workspace = this.createWorkspaceOn(this.connection);
    return true;
  }
}
