import { InvokeResult as ConnectionInvokeResult } from '@allors/system/workspace/connection';
import { IInvokeResult, ISession } from '@allors/system/workspace/domain';
import { Result } from './result';

export class InvokeResult extends Result implements IInvokeResult {
  constructor(session: ISession, invoked: ConnectionInvokeResult) {
    super(session, invoked);
  }
}
