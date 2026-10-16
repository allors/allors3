import {
  CreationMapping,
  IObject,
  IPendingPull,
  IPullResult,
  IPushResult,
  PushDiscovery,
} from '@allors/system/workspace/domain';
import { RelationType } from '@allors/system/workspace/meta';
import {
  MutationResultState,
  ScopedMutationResult,
} from './scoped-mutation-result';

export interface PushResultState extends MutationResultState {
  creationMappings: readonly CreationMapping[];
  discovery: PushDiscovery;
  pendingQuery?: IPendingPull;
}

export class ScopedPushResult
  extends ScopedMutationResult
  implements IPushResult
{
  readonly creationMappings: readonly CreationMapping[];
  readonly discovery: PushDiscovery;
  readonly pendingQuery?: IPendingPull;
  constructor(
    state: PushResultState,
    held: (id: number) => IObject,
    relation: (tag: string) => RelationType
  ) {
    super(state, held, relation);
    this.creationMappings = Object.freeze(
      state.creationMappings.map((mapping) => Object.freeze({ ...mapping }))
    );
    this.discovery = Object.freeze({
      ...state.discovery,
      errors: Object.freeze([...state.discovery.errors]),
    });
    this.pendingQuery = state.pendingQuery;
    Object.freeze(this);
  }
  override get hasErrors(): boolean {
    return (
      super.hasErrors ||
      ['failed', 'unavailable'].includes(this.discovery.status)
    );
  }
  override get errorMessage(): string {
    return [
      this.diagnostics.errorMessage,
      ...this.diagnostics.errors.map((error) => error.message),
      ...this.diagnostics.derivationErrors.map((error) => error.message),
      ...this.refresh.errors.map((error) => error.message),
      ...this.discovery.errors.map((error) => error.message),
      ...this.application.errors.map((error) => error.message),
    ]
      .filter(Boolean)
      .join('; ');
  }
}

/** Owns detached query input without retaining a transaction, gate or cache pin. */
export class PendingPull implements IPendingPull {
  #used = false;
  constructor(
    private readonly completion: (
      cancellation?: AbortSignal
    ) => Promise<IPullResult>
  ) {}
  async complete(cancellation?: AbortSignal): Promise<IPullResult> {
    if (this.#used)
      throw new Error(
        'Pending query can be completed only once. Start a new Pull.'
      );
    this.#used = true;
    return this.completion(cancellation);
  }
}
