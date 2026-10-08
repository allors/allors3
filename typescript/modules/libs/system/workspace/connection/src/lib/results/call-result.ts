import { Response } from '@allors/system/common/protocol-json';
import { MetaPopulation } from '@allors/system/workspace/meta';
import { DerivationError } from './derivation-error';

export function responseHasErrors(response: Response): boolean {
  return (
    response._v?.length > 0 ||
    response._a?.length > 0 ||
    response._m?.length > 0 ||
    response._d?.length > 0 ||
    !!response._e
  );
}

/**
 * What the server answered to a pull, a push or an invoke: the errors, in ids.
 */
export abstract class CallResult {
  private _derivationErrors: DerivationError[];

  protected constructor(
    protected readonly metaPopulation: MetaPopulation,
    private readonly response: Response
  ) {}

  get hasErrors(): boolean {
    return responseHasErrors(this.response);
  }

  get errorMessage(): string {
    return this.response._e;
  }

  /**
   * The ids of the objects whose version differed from the server's.
   */
  get versionErrors(): number[] {
    return this.response._v ?? [];
  }

  /**
   * The ids of the objects the user may not access as requested.
   */
  get accessErrors(): number[] {
    return this.response._a ?? [];
  }

  /**
   * The ids of the objects the server does not have.
   */
  get missingErrors(): number[] {
    return this.response._m ?? [];
  }

  get derivationErrors(): DerivationError[] {
    return (this._derivationErrors ??=
      this.response._d?.map(
        (v) => new DerivationError(this.metaPopulation, v)
      ) ?? []);
  }
}
