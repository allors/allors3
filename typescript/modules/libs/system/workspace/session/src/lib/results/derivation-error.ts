import { DerivationError as ConnectionDerivationError } from '@allors/system/workspace/connection';
import {
  IDatabaseDerivationError,
  ISession,
  Role,
} from '@allors/system/workspace/domain';

export class DerivationError implements IDatabaseDerivationError {
  constructor(
    private readonly session: ISession,
    private readonly error: ConnectionDerivationError
  ) {}

  get message() {
    return this.error.message;
  }

  get roles(): Role[] {
    return this.error.roles.map((v) => {
      return {
        object: this.session.instantiate(v.objectId),
        relationType: v.relationType,
      } as Role;
    });
  }
}
