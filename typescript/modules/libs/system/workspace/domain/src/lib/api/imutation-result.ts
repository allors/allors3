import { IPullResult } from './pull/ipull-result';

export type MutationOutcome = 'committed' | 'rejected' | 'unknown';
export type MutationReadStatus =
  | 'not-requested'
  | 'unavailable'
  | 'succeeded'
  | 'failed';
export type MutationAcknowledgementStatus =
  | 'not-applicable'
  | 'applied'
  | 'failed';
export type MutationRefreshApplicationStatus =
  | 'not-applicable'
  | 'skipped'
  | 'applied'
  | 'conflicted'
  | 'failed';
export type MutationApplicationStage =
  | 'acknowledgement'
  | 'refresh'
  | 'discovery'
  | 'notification';

export interface MutationDiagnostic {
  readonly code: string;
  readonly message: string;
  readonly objectIds: readonly number[];
  readonly selectionIndex?: number;
  readonly cause?: Error;
}
export interface MutationDerivationRelation {
  readonly objectId: number;
  readonly relationTypeTag: string;
}
export interface MutationDerivationDiagnostic {
  readonly message?: string | null;
  readonly relations: readonly MutationDerivationRelation[];
}
export interface MutationDiagnostics {
  readonly errorMessage?: string | null;
  readonly errors: readonly MutationDiagnostic[];
  readonly versionErrorIds: readonly number[];
  readonly accessErrorIds: readonly number[];
  readonly missingErrorIds: readonly number[];
  readonly derivationErrors: readonly MutationDerivationDiagnostic[];
}
export interface CreationMapping {
  readonly workspaceId: number;
  readonly databaseId: number;
}
export interface MutationRefresh {
  readonly status: MutationReadStatus;
  readonly result?: IPullResult;
  readonly errors: readonly MutationDiagnostic[];
}
export interface MutationApplicationError {
  readonly stage: MutationApplicationStage;
  readonly message: string;
  readonly cause?: Error;
}
export interface MutationApplication {
  readonly acknowledgement: MutationAcknowledgementStatus;
  readonly refresh: MutationRefreshApplicationStatus;
  readonly errors: readonly MutationApplicationError[];
}
export interface IMutationResult {
  readonly outcome: MutationOutcome;
  readonly diagnostics: MutationDiagnostics;
  readonly refresh: MutationRefresh;
  readonly application: MutationApplication;
  readonly reconciliationRequired: boolean;
}
