import { Workspace as SystemWorkspace } from '@allors/system/workspace/adapters';
import { DatabaseConnection } from '../database/database-connection';
import { Session } from '../session/session';

export class Workspace extends SystemWorkspace {
  constructor(database: DatabaseConnection) {
    database.assertActive();
    super(database);
  }

  createSession(): Session {
    return new Session(this);
  }
}
