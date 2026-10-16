import { ReadError, Response } from '@allors/system/common/protocol-json';
import {
  IDatabaseDerivationError,
  IInvokeResult,
  IObject,
  MutationApplication,
  MutationDiagnostics,
  MutationDiagnostic,
  MutationOutcome,
  MutationRefresh,
} from '@allors/system/workspace/domain';
import { RelationType } from '@allors/system/workspace/meta';

export interface MutationResultState {
  outcome: MutationOutcome;
  diagnostics: MutationDiagnostics;
  refresh: MutationRefresh;
  application: MutationApplication;
  reconciliationRequired: boolean;
}

export function mutationDiagnostic(
  code: string,
  cause: unknown
): MutationDiagnostic {
  const error = cause instanceof Error ? cause : new Error(String(cause));
  return Object.freeze({
    code,
    message:
      code === 'push-outcome-unknown' || code === 'invoke-outcome-unknown'
        ? `${
            code === 'invoke-outcome-unknown' ? 'The invocation' : 'The save'
          } may have committed. Inspect server state and use a fresh Session after reconciliation; do not replay automatically. ${
            error.message
          }`
        : error.message ||
          'Inspect the operation outcome before another mutation.',
    objectIds: Object.freeze([] as number[]),
    cause: error,
  });
}

export function readDiagnostics(
  errors: readonly (Omit<ReadError, 'o'> & {
    readonly o?: readonly number[];
  })[],
  cause?: Error
): readonly MutationDiagnostic[] {
  return Object.freeze(
    errors.map((error) =>
      Object.freeze({
        code: error.c,
        message: error.m,
        objectIds: Object.freeze([...(error.o ?? [])]),
        ...(error.i == null ? {} : { selectionIndex: error.i }),
        ...(cause ? { cause } : {}),
      })
    )
  );
}

export function mutationDiagnostics(
  response: Partial<Response> & { e?: ReadError[] },
  outcome: MutationOutcome,
  client: readonly MutationDiagnostic[] = [],
  operation: 'push' | 'invoke' = 'push'
): MutationDiagnostics {
  const errors = [...readDiagnostics(response.e ?? []), ...client];
  const derivationErrors = (response._d ?? []).map((error) =>
    Object.freeze({
      ...(error.m === undefined ? {} : { message: error.m }),
      relations: Object.freeze(
        error.r.map((relation) =>
          Object.freeze({ objectId: relation.i, relationTypeTag: relation.r })
        )
      ),
    })
  );
  if (
    outcome !== 'committed' &&
    !response._e?.trim() &&
    !errors.some((error) => error.message.trim()) &&
    !derivationErrors.some((error) => error.message?.trim())
  ) {
    errors.push(
      mutationDiagnostic(
        `${operation}-${outcome}`,
        new Error(
          outcome === 'rejected'
            ? `${
                operation === 'invoke' ? 'The invocation' : 'The save'
              } was rejected. Inspect its diagnostics and correct the submitted changes.`
            : `${
                operation === 'invoke' ? 'The invocation' : 'The save'
              } may have committed. Inspect server state and use a fresh Session after reconciliation; do not replay automatically.`
        )
      )
    );
  }
  return Object.freeze({
    ...(response._e === undefined ? {} : { errorMessage: response._e }),
    errors: Object.freeze(errors),
    versionErrorIds: Object.freeze([...(response._v ?? [])]),
    accessErrorIds: Object.freeze([...(response._a ?? [])]),
    missingErrorIds: Object.freeze([...(response._m ?? [])]),
    derivationErrors: Object.freeze(derivationErrors),
  });
}

/** Immutable mutation snapshot; raw diagnostic IDs never require loading an editing object. */
export class ScopedMutationResult implements IInvokeResult {
  readonly outcome: MutationOutcome;
  readonly diagnostics: MutationDiagnostics;
  readonly refresh: MutationRefresh;
  readonly application: MutationApplication;
  readonly reconciliationRequired: boolean;
  private readonly versions: IObject[];
  private readonly access: IObject[];
  private readonly missing: IObject[];
  private readonly derivations: IDatabaseDerivationError[];

  constructor(
    state: MutationResultState,
    held: (id: number) => IObject,
    relation: (tag: string) => RelationType
  ) {
    this.outcome = state.outcome;
    this.diagnostics = state.diagnostics;
    this.refresh = Object.freeze({
      ...state.refresh,
      errors: Object.freeze([...state.refresh.errors]),
    });
    this.application = Object.freeze({
      ...state.application,
      errors: Object.freeze(
        state.application.errors.map((error) => Object.freeze({ ...error }))
      ),
    });
    this.reconciliationRequired = state.reconciliationRequired;
    const objects = (ids: readonly number[]) =>
      ids.map(held).filter((object) => object != null);
    this.versions = objects(this.diagnostics.versionErrorIds);
    this.access = objects(this.diagnostics.accessErrorIds);
    this.missing = objects(this.diagnostics.missingErrorIds);
    this.derivations = this.diagnostics.derivationErrors.map((error) => ({
      message: error.message,
      roles: error.relations
        .map((entry) => {
          const object = held(entry.objectId);
          const relationType = relation(entry.relationTypeTag);
          return object && relationType ? { object, relationType } : undefined;
        })
        .filter((role) => role != null),
    }));
  }

  get hasErrors(): boolean {
    return (
      this.outcome !== 'committed' ||
      !!this.diagnostics.errorMessage ||
      this.diagnostics.errors.length > 0 ||
      this.diagnostics.versionErrorIds.length > 0 ||
      this.diagnostics.accessErrorIds.length > 0 ||
      this.diagnostics.missingErrorIds.length > 0 ||
      this.diagnostics.derivationErrors.length > 0 ||
      ['failed', 'unavailable'].includes(this.refresh.status) ||
      this.application.errors.length > 0 ||
      this.application.refresh === 'conflicted' ||
      (this.refresh.result?.mergeErrors?.length ?? 0) > 0
    );
  }
  get errorMessage(): string {
    return [
      this.diagnostics.errorMessage,
      ...this.diagnostics.errors.map((error) => error.message),
      ...this.diagnostics.derivationErrors.map((error) => error.message),
      ...this.refresh.errors.map((error) => error.message),
      ...this.application.errors.map((error) => error.message),
    ]
      .filter(Boolean)
      .join('; ');
  }
  get versionErrors(): IObject[] {
    return [...this.versions];
  }
  get accessErrors(): IObject[] {
    return [...this.access];
  }
  get missingErrors(): IObject[] {
    return [...this.missing];
  }
  get derivationErrors(): IDatabaseDerivationError[] {
    return this.derivations.map((error) => ({
      message: error.message,
      roles: error.roles.map((role) => ({ ...role })),
    }));
  }
}

export class ScopedInvokeResult extends ScopedMutationResult {
  constructor(
    state: MutationResultState,
    held: (id: number) => IObject,
    relation: (tag: string) => RelationType
  ) {
    super(state, held, relation);
    Object.freeze(this);
  }
}
