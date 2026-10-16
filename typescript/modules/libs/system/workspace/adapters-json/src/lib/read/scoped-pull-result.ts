import { ISession } from '@allors/system/workspace/domain';
import { ReadInput, PullResponse } from '@allors/system/common/protocol-json';
import { PullResult as WirePullResult } from '../database/pull/pull-result';
import { ReadFailedError } from './read-failed-error';
import { ReadStoreScope } from './read-store-values';

/** Named results contain live editing objects; diagnostics describe the completed read. */
export class PullResult extends WirePullResult {
  readonly scope?: ReadStoreScope;
  readonly outcomes: readonly Readonly<ReadInput>[];
  readonly readErrors: ReadFailedError['errors'];

  constructor(
    session: ISession,
    readonly requestId: string,
    completed?: {
      readonly scope: ReadStoreScope;
      readonly outcomes: readonly Readonly<ReadInput>[];
      readonly collections: Readonly<Record<string, readonly number[]>>;
      readonly objects: Readonly<Record<string, number | null>>;
    },
    failure?: ReadFailedError
  ) {
    super(session, {
      c: completed
        ? Object.keys(completed.collections).reduce((collections, name) => {
            collections[name] = [...completed.collections[name]];
            return collections;
          }, Object.create(null) as Record<string, number[]>)
        : {},
      o: completed ? { ...completed.objects } : {},
    } as PullResponse);
    this.scope = completed?.scope ?? failure?.scope;
    this.outcomes = Object.freeze(
      (completed?.outcomes ?? []).map((outcome) =>
        Object.freeze({ ...outcome })
      )
    );
    this.readErrors = failure?.errors ?? Object.freeze([]);
  }

  override get hasErrors(): boolean {
    return super.hasErrors || this.readErrors.length > 0;
  }

  override get errorMessage(): string {
    return (
      this.readErrors.map((error) => error.m).join('; ') || super.errorMessage
    );
  }
}
