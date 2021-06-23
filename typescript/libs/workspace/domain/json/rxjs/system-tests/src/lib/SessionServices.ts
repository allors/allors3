import { ISession, ISessionServices } from '@allors/workspace/system';

export class SessionServices implements ISessionServices {
  session: ISession;

  onInit(session: ISession): void {
    this.session = session;
  }
}
