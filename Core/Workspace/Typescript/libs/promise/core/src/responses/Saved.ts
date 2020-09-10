import { Session } from '@allors/domain/system';
import { PushResponse } from '@allors/protocol/system';

export class Saved {
  constructor(public session: Session, public response: PushResponse) {}
}
