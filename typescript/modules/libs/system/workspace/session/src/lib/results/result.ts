import { CallResult } from '@allors/system/workspace/connection';
import {
  IDatabaseDerivationError,
  IObject,
  IResult,
  ISession,
} from '@allors/system/workspace/domain';
import { DerivationError } from './derivation-error';

/**
 * A result of the connection with its ids turned into the objects of a session.
 */
export abstract class Result implements IResult {
  private _derivationErrors: IDatabaseDerivationError[];

  protected constructor(
    public readonly session: ISession,
    protected readonly result: CallResult
  ) {}

  get hasErrors(): boolean {
    return this.result.hasErrors;
  }

  get errorMessage(): string {
    return this.result.errorMessage;
  }

  get versionErrors(): IObject[] {
    return this.session.instantiate(this.result.versionErrors);
  }

  get accessErrors(): IObject[] {
    return this.session.instantiate(this.result.accessErrors);
  }

  get missingErrors(): IObject[] {
    return this.session.instantiate(this.result.missingErrors);
  }

  get derivationErrors(): IDatabaseDerivationError[] {
    return (this._derivationErrors ??= this.result.derivationErrors.map(
      (v) => new DerivationError(this.session, v)
    ));
  }
}
