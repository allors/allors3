import { IInvokeResult, ISession } from '@allors/workspace/system';
import { InvokeResponse } from '@allors/protocol/json/system';
import { Result } from '../Result';

export class InvokeResult extends Result implements IInvokeResult {
  constructor(session: ISession, invokeResponse: InvokeResponse) {
    super(session, invokeResponse);
  }
}
