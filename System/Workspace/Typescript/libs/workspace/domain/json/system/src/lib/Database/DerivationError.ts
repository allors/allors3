import { ResponseDerivationError } from '@allors/protocol/json/system';
import { IDerivationError, ISession, Role } from '@allors/workspace/domain/system';

export class DerivationError implements IDerivationError {
  constructor(private session: ISession, private responseDerivationError: ResponseDerivationError) {}

  get message() {
    return this.responseDerivationError.m;
  }

  get roles(): Role[] {
    // from r in this.responseDerivationError.Roles
    // let association = this.session.Get<IObject>(r[0])
    // let relationType = (IRelationType)this.session.Workspace.MetaPopulation.FindByTag((int)r[1])
    // select new Role(association, relationType);

    // TODO:
    return [];
  }
}
