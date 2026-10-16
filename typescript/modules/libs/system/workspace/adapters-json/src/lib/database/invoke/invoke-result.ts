import {
  IInvokeResult,
  IObject,
  MutationOutcome,
} from '@allors/system/workspace/domain';
import { InvokeResponse } from '@allors/system/common/protocol-json';
import {
  mutationDiagnostics,
  ScopedMutationResult,
} from '../../read/scoped-mutation-result';
import { Session } from '../../session/session';
import { RelationType } from '@allors/system/workspace/meta';

/** Interim legacy adaptation; transport exceptions do not carry protected receipt guarantees. */
export class InvokeResult
  extends ScopedMutationResult
  implements IInvokeResult
{
  constructor(
    session: Session,
    response: InvokeResponse,
    held: (id: number) => IObject
  ) {
    const outcome: MutationOutcome =
      !!response._e ||
      !!response._v?.length ||
      !!response._a?.length ||
      !!response._m?.length ||
      !!response._d?.length
        ? 'rejected'
        : 'committed';
    super(
      {
        outcome,
        diagnostics: mutationDiagnostics(response, outcome, [], 'invoke'),
        refresh: { status: 'not-requested', errors: [] },
        application: {
          acknowledgement: 'not-applicable',
          refresh: 'not-applicable',
          errors: [],
        },
        reconciliationRequired: false,
      },
      held,
      (tag) =>
        session.workspace.configuration.metaPopulation.metaObjectByTag.get(
          tag
        ) as RelationType
    );
    Object.freeze(this);
  }
}
