import { IInvokeResult, ISession } from '@allors/system/workspace/domain';
import { InvokeResponse } from '@allors/system/common/protocol-json';
import { Result } from '../result';

export class InvokeResult extends Result implements IInvokeResult {
  constructor(session: ISession, invokeResponse: InvokeResponse) {
    super(session, invokeResponse);
  }
}
