import { Session } from '@allors/workspace/core';
import { InvokeResponse } from '@allors/protocol/core';

export class Invoked {
  constructor(public session: Session, public response: InvokeResponse) {}
}
