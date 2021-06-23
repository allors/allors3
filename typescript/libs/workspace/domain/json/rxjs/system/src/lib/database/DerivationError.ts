import { ResponseDerivationError } from '@allors/protocol/json/system';
import { IDerivationError, ISession, Role } from '@allors/workspace/system';

export class DerivationError implements IDerivationError {
  constructor(private session: ISession, private responseDerivationError: ResponseDerivationError) {}

  get message() {
    return this.responseDerivationError.m;
  }

  get roles(): Role[] {
    return this.responseDerivationError.r.map((r) => {
      return {
        object: this.session.getOne(r[0]),
        relationType: this.session.workspace.database.configuration.metaPopulation.metaObjectByTag.get(r[1]),
      } as Role;
    });
  }
}
