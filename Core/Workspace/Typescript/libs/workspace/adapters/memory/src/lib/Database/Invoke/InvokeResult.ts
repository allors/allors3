export class InvokeResult extends Result implements IInvokeResult {
  constructor(session: ISession, invokeResponse: InvokeResponse) {
    super(session, invokeResponse);
  }
}
