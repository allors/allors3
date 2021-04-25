import { Session } from '@allors/workspace/core';
import { PushResponse } from '@allors/protocol/json/system';

export class Saved {
  constructor(public session: Session, public response: PushResponse) {}
}
