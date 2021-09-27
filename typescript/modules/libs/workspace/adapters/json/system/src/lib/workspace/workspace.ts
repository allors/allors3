import { Workspace as SystemWorkspace } from '@allors/workspace/adapters/system';
import { ISession, IWorkspaceServices } from '@allors/workspace/domain/system';
import { DatabaseConnection } from '../database/database-connection';
import { Session } from '../session/Session';

export class Workspace extends SystemWorkspace {

  constructor(database: DatabaseConnection, services: IWorkspaceServices) {
    super(database, services);
    
    this.services.onInit(this);
  }

  createSession(): ISession {
    return new Session(this, this.services.createSessionServices());
  }
}
