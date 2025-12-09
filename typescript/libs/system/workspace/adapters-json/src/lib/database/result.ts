import { Response } from '@allors/system/common/protocol-json';
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
    return this.session.instantiate(this.response._v);
  }

  get accessErrors(): IObject[] {
    return this.session.instantiate(this.response._a);
  }

  get missingErrors(): IObject[] {
    return this.session.instantiate(this.response._m);
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
