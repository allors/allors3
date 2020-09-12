import { Session } from '@allors/workspace/system';
import { InvokeResponse } from '@allors/protocol/system';

export class Invoked {
  constructor(public session: Session, public response: InvokeResponse) {}
}
