import { ISession } from '@allors/workspace/domain/system';
import { InvokeResponse } from '@allors/protocol/system';

export class Invoked {
  constructor(public session: ISession, public response: InvokeResponse) {}
}
