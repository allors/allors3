import { IResult } from '../iresult';
import {
  CreationMapping,
  IMutationResult,
  MutationDiagnostic,
  MutationReadStatus,
} from '../imutation-result';
import { IPullResult } from '../pull/ipull-result';

export interface PushDiscovery {
  readonly status: MutationReadStatus;
  readonly errors: readonly MutationDiagnostic[];
}
export interface IPendingPull {
  complete(cancellation?: AbortSignal): Promise<IPullResult>;
}
export interface IPushResult extends IResult, IMutationResult {
  readonly creationMappings: readonly CreationMapping[];
  readonly discovery: PushDiscovery;
  readonly pendingQuery?: IPendingPull;
}
