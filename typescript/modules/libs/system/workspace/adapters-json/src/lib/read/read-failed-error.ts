import { ReadFailure } from '@allors/system/common/protocol-json';
import { ReadStoreScope } from './read-store-values';

/** Failed read diagnostics preserve protocol IDs without constructing Session objects. */
export class ReadFailedError extends Error {
  readonly requestId: string;
  readonly scope?: ReadStoreScope;
  readonly errors: readonly {
    readonly c: string;
    readonly m: string;
    readonly o?: readonly number[] | null;
    readonly i?: number | null;
  }[];

  constructor(response: ReadFailure) {
    super(
      `Read failed: ${response.e
        .map((entry) => `${entry.c}: ${entry.m}`)
        .join('; ')}`
    );
    this.name = 'ReadFailedError';
    this.requestId = response.i;
    if (response.sc)
      this.scope = new ReadStoreScope(
        response.sc.p,
        response.sc.u,
        response.sc.w,
        response.sc.r,
        response.sc.x ?? null
      );
    this.errors = Object.freeze(
      response.e.map((entry) =>
        Object.freeze({
          ...entry,
          ...(entry.o ? { o: Object.freeze([...entry.o]) } : {}),
        })
      )
    );
    Object.freeze(this);
  }
}
