import { PushResponse } from '@allors/system/common/protocol-json';
import { IPushResult, ISession } from '@allors/system/workspace/domain';
import { Result } from '../result';

export class PushResult extends Result implements IPushResult {
  constructor(session: ISession, response: PushResponse) {
    super(session, response);
  }
}
