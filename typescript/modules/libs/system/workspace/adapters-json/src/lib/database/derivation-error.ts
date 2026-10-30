import { ResponseDerivationError } from '@allors/system/common/protocol-json';
import { Session } from '@allors/system/workspace/adapters';
import {
  IDatabaseDerivationError,
  ISession,
  Role,
} from '@allors/system/workspace/domain';

export class DerivationError implements IDatabaseDerivationError {
  constructor(
    private session: ISession,
    private responseDerivationError: ResponseDerivationError
  ) {}

  get message() {
    return this.responseDerivationError.m;
  }

  get roles(): Role[] {
    return this.responseDerivationError.r.map((r) => {
      return {
        // A derivation error may concern an object this session does not hold.
        object: (this.session as Session).getObject(r[0]),
        relationType:
          this.session.workspace.configuration.metaPopulation.metaObjectByTag.get(
            r[1]
          ),
      } as Role;
    });
  }
}
