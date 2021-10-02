import { Workspace as SystemWorkspace } from '@allors/workspace/adapters/system';
import { ISession } from '@allors/workspace/domain/system';
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
