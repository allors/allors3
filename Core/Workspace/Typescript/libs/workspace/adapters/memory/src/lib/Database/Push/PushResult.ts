import { IPushResult, ISession } from "@allors/workspace/domain/system";
import { Result } from "../Result";

export class PushResult extends Result implements IPushResult {
  constructor(session: ISession, response: Response) {
    super(session, response);
  }
}
