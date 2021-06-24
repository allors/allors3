import { Workspace as SystemWorkspace } from '@allors/workspace/adapters/system';
import { ISession } from '@allors/workspace/domain/system';
import { Session } from './Session';

export class Workspace extends SystemWorkspace {
  createSession(): ISession {
    return new Session(this, this.services.createSessionServices());
  }
}
