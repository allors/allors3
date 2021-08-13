import { ResponseDerivationError } from '@allors/protocol/json/system';
import { IDatabaseDerivationError, ISession, Role } from '@allors/workspace/domain/system';

export class DerivationError implements IDatabaseDerivationError {
  constructor(private session: ISession, private responseDerivationError: ResponseDerivationError) {}

  get message() {
    return this.responseDerivationError.m;
  }

  get roles(): Role[] {
    return this.responseDerivationError.r.map((r) => {
      return {
        object: this.session.instantiate(r[0]),
        relationType: this.session.workspace.configuration.metaPopulation.metaObjectByTag.get(r[1]),
      } as Role;
    });
  }
}
