import { Workspace as SystemWorkspace } from '@allors/system/workspace/adapters';
import { ISession } from '@allors/system/workspace/domain';
import { DatabaseConnection } from '../database/database-connection';
import { Session } from '../session/session';

export class Workspace extends SystemWorkspace {
  constructor(database: DatabaseConnection) {
    super(database);
  }

  createSession(): ISession {
    return new Session(this);
  }
}
