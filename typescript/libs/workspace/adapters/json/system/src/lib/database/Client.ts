import { PullRequest, PullResponse } from '@allors/protocol/json/system';
import { Observable } from 'rxjs';

export interface Client {
  baseUrl: string;
  userId: number;
  jwtToken: string;

  pull(pullRequest: PullRequest): Observable<PullResponse>;
}
