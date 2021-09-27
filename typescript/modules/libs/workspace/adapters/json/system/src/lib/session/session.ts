import { Session as SystemSession } from '@allors/workspace/adapters/system';
import { IObject, ISessionServices } from '@allors/workspace/domain/system';
import { Class, Origin } from '@allors/workspace/meta/system';
import { DatabaseConnection } from '../database/database-connection';
import { DatabaseRecord } from '../database/database-record';
import { Workspace } from '../workspace/Workspace';
import { Strategy } from './Strategy';

export class Session extends SystemSession {
  database: DatabaseConnection;

  constructor(workspace: Workspace, services: ISessionServices) {
    super(workspace, services);

    this.services.onInit(this);
    this.database = this.workspace.database as DatabaseConnection;
  }
 
  create<T extends IObject>(cls: Class): T {
    const workspaceId = this.workspace.database.nextId();
    const strategy = new Strategy(this, cls, workspaceId);
    this.addStrategy(strategy);

    if (cls.origin != Origin.Session) {
      this.pushToWorkspaceTracker.onCreated(strategy);
      if (cls.origin == Origin.Database) {
        this.pushToDatabaseTracker.onCreated(strategy);
      }
    }

    this.changeSetTracker.onCreated(strategy);
    return strategy.object as T;
  }

  instantiateDatabaseStrategy(id: number): void {
    const databaseRecord = this.workspace.database.getRecord(id) as DatabaseRecord;
    const strategy = Strategy.fromDatabaseRecord(this, databaseRecord);
    this.addStrategy(strategy);
    this.changeSetTracker.onInstantiated(strategy);
  }

  instantiateWorkspaceStrategy(id: number): Strategy {
    if (!this.workspace.workspaceClassByWorkspaceId.has(id)) {
      return null;
    }

    const cls = this.workspace.workspaceClassByWorkspaceId.get(id);

    const strategy = new Strategy(this, cls, id);
    this.addStrategy(strategy);

    this.changeSetTracker.onInstantiated(strategy);

    return strategy;
  }
}
