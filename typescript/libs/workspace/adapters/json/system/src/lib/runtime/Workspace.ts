import { Workspace as SystemWorkspace } from '@allors/workspace/adapters/system';
import { ISession, IWorkspaceServices } from '@allors/workspace/domain/system';
import { Database } from './Database';
import { Session } from './Session';

export class Workspace extends SystemWorkspace {

  constructor(database: Database, services: IWorkspaceServices) {
    super(database, services);
    
    this.services.onInit(this);
  }

  createSession(): ISession {
    return new Session(this, this.services.createSessionServices());
  }
}
