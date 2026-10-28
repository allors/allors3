import { PullRequest } from '../pull/pull-request';
import { PullResponseObject } from '../pull/pull-response-object';
import { Request } from '../request';

export interface SyncRequest extends Request {
  pull: PullRequest;
  o: PullResponseObject[];
  g: number[][];
  r: number[][];
}
