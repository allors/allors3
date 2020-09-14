import { Session } from '@allors/workspace/system';
import { PushResponse } from '@allors/protocol/system';

export class Saved {
  constructor(public session: Session, public response: PushResponse) {}
}
