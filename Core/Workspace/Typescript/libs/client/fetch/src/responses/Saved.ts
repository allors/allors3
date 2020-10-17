import { Session } from '@allors/workspace/core';
import { PushResponse } from '@allors/protocol/core';

export class Saved {
  constructor(public session: Session, public response: PushResponse) {}
}
