import { Response } from '@allors/protocol/json/system';
import { IDerivationError, IObject, IResult, ISession } from '@allors/workspace/domain/system';
import { DerivationError } from './DerivationError';

export abstract class Result implements IResult {
  constructor(public readonly session: ISession, public readonly response: Response) {}

  get hasErrors(): boolean {
    return this.response.v?.length > 0 || this.response.a?.length > 0 || this.response.m?.length > 0 || this.response.d?.length > 0 || !!this.response.e;
  }

  get errorMessage(): string {
    return this.response.e;
  }

  get versionErrors(): IObject[] {
    return this.session.getMany<IObject>(this.response.v);
  }

  get accessErrors(): IObject[] {
    return this.session.getMany<IObject>(this.response.a);
  }

  get missingErrors(): IObject[] {
    return this.session.getMany<IObject>(this.response.m);
  }

  get derivationErrors(): IDerivationError[] {
    return this.response.d?.map((v) => new DerivationError(this.session, v)) ?? [];
  }
}
