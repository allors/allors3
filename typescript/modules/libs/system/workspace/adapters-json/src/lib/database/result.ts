import { Response } from '@allors/system/common/protocol-json';
import { Session } from '@allors/system/workspace/adapters';
import {
  IDatabaseDerivationError,
  IObject,
  IResult,
  ISession,
} from '@allors/system/workspace/domain';
import { DerivationError } from './derivation-error';

export abstract class Result implements IResult {
  private _derivationErrors: IDatabaseDerivationError[];

  constructor(
    public readonly session: ISession,
    public readonly response: Response
  ) {}

  get hasErrors(): boolean {
    return (
      this.response._v?.length > 0 ||
      this.response._a?.length > 0 ||
      this.response._m?.length > 0 ||
      this.response._d?.length > 0 ||
      !!this.response._e
    );
  }

  get errorMessage(): string {
    return this.response._e;
  }

  get versionErrors(): IObject[] {
    return this.heldObjects(this.response._v);
  }

  get accessErrors(): IObject[] {
    return this.heldObjects(this.response._a);
  }

  get missingErrors(): IObject[] {
    return this.heldObjects(this.response._m);
  }

  // The server may report objects this session does not hold, such as removed role members.
  private heldObjects(ids: number[]): IObject[] {
    if (ids == null) {
      return null;
    }

    return ids
      .map((v) => (this.session as Session).getObject(v))
      .filter((v) => v != null);
  }

  get derivationErrors(): IDatabaseDerivationError[] {
    if (this._derivationErrors != null) {
      return this._derivationErrors;
    }

    this._derivationErrors =
      this.response._d?.map((v) => new DerivationError(this.session, v)) ?? [];

    return this._derivationErrors;
  }
}
