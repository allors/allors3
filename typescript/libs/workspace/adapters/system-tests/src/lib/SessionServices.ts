import { ISession, ISessionServices, IValidation } from '@allors/workspace/domain/system';

export class SessionServices implements ISessionServices {
  session: ISession;

  onInit(session: ISession): void {
    this.session = session;
  }

  derive(): IValidation {
    const derivationService = this.session.workspace.services.derivationService;
    const derivation = derivationService.create(this.session);
    return derivation.execute();
  }
}
