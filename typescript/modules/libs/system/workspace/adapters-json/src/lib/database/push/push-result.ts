import { PushResponse } from '@allors/system/common/protocol-json';
import {
  CreationMapping,
  IPushResult,
  ISession,
  MutationApplication,
  MutationDiagnostics,
  MutationOutcome,
  MutationRefresh,
  PushDiscovery,
} from '@allors/system/workspace/domain';
import { Result } from '../result';
import { mutationDiagnostics } from '../../read/scoped-mutation-result';

/** Legacy response adaptation; protected admission and uncertainty handling belong to scoped Push. */
export class PushResult extends Result implements IPushResult {
  readonly outcome: MutationOutcome;
  readonly diagnostics: MutationDiagnostics;
  readonly refresh: MutationRefresh = Object.freeze({
    status: 'not-requested',
    errors: Object.freeze([]),
  });
  readonly discovery: PushDiscovery = Object.freeze({
    status: 'not-requested',
    errors: Object.freeze([]),
  });
  readonly application: MutationApplication;
  readonly reconciliationRequired = false;
  readonly creationMappings: readonly CreationMapping[];

  constructor(
    session: ISession,
    response: PushResponse,
    expectedIds: readonly number[],
    held: (id: number) => boolean
  ) {
    super(session, response);
    if (!response || typeof response !== 'object' || Array.isArray(response))
      throw new Error(
        'Invalid Push acknowledgement. The save may have committed; inspect server state and do not replay automatically.'
      );
    this.outcome = this.hasErrors ? 'rejected' : 'committed';
    this.diagnostics = mutationDiagnostics(response, this.outcome);
    this.application = Object.freeze({
      acknowledgement:
        this.outcome === 'committed' ? 'applied' : 'not-applicable',
      refresh: 'not-applicable',
      errors: Object.freeze([]),
    });
    if (this.outcome === 'rejected') {
      this.creationMappings = Object.freeze([]);
      return;
    }
    const mappings = response.n ?? [];
    const workspaceIds = new Set<number>();
    const databaseIds = new Set<number>();
    if (
      !Array.isArray(mappings) ||
      mappings.length !== expectedIds.length ||
      mappings.some((mapping) => {
        if (
          !mapping ||
          !Number.isSafeInteger(mapping.w) ||
          mapping.w >= 0 ||
          !expectedIds.includes(mapping.w) ||
          workspaceIds.has(mapping.w) ||
          !held(mapping.w) ||
          !Number.isSafeInteger(mapping.d) ||
          mapping.d <= 0 ||
          databaseIds.has(mapping.d) ||
          held(mapping.d)
        )
          return true;
        workspaceIds.add(mapping.w);
        databaseIds.add(mapping.d);
        return false;
      })
    )
      throw new Error(
        'Invalid or incomplete Push creation mappings. The save may have committed; inspect server state and do not replay automatically.'
      );
    this.creationMappings = Object.freeze(
      mappings.map((mapping) =>
        Object.freeze({ workspaceId: mapping.w, databaseId: mapping.d })
      )
    );
  }
}
