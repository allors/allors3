import { PushResult as ConnectionPushResult } from '@allors/system/workspace/connection';
import { IPushResult, ISession } from '@allors/system/workspace/domain';
import { Result } from './result';

export class PushResult extends Result implements IPushResult {
  constructor(session: ISession, pushed: ConnectionPushResult) {
    super(session, pushed);
  }
}
