import { IPullResult, Pull } from '@allors/workspace/domain/system';
import { Observable } from 'rxjs';

export interface Context {
  pull(pullOrPulls: Pull | Pull[]): Observable<IPullResult>;
}
