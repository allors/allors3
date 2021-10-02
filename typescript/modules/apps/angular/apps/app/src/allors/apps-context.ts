import { Context } from '@allors/workspace/angular/core';
import { IPullResult, IReactiveDatabaseClient, ISession, Pull } from '@allors/workspace/domain/system';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export class AppsContext implements Context {
  constructor(public session: ISession, public client: IReactiveDatabaseClient) {}

  pull(pulls: Pull[]): Observable<IPullResult> {
    return this.client.pullReactive(this.session, pulls).pipe(tap(() => this.session.derive()));
  }
}
