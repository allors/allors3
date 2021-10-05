import { from, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Context, WorkspaceService } from '@allors/workspace/angular/core';
import { IConfiguration, IInvokeResult, InvokeOptions, IObject, IPullResult, IDatabaseClient, IResult, ISession, IWorkspace, Method, Pull } from '@allors/workspace/domain/system';
import { Class, Composite } from '@allors/workspace/meta/system';

export class AppsContext implements Context {
  constructor(public workspaceService: WorkspaceService) {
    this.workspace = this.workspaceService.workspace;
    this.client = this.workspaceService.client;
    this.configuration = this.workspace.configuration;
    this.session = this.workspace.createSession();
  }

  workspace: IWorkspace;

  client: IDatabaseClient;

  configuration: IConfiguration;

  session: ISession;

  create<T extends IObject>(cls: Class): T {
    return this.session.create<T>(cls);
  }

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];
  instantiate<T extends IObject>(arg: any): T | T[] {
    return this.session.instantiate<T>(arg);
  }

  reset(): void {
    this.session.reset();
  }

  hasChanges(): boolean {
    return this.session.hasChanges;
  }

  pull(pulls: Pull[]): Observable<IPullResult> {
    return from(this.client.pull(this.session, pulls)).pipe(tap(() => this.session.derive()));
  }

  push(): Observable<IResult> {
    return from(this.client.push(this.session));
  }

  invoke(methods: Method | Method[], options?: InvokeOptions): Observable<IInvokeResult> {
    return from(this.client.invoke(this.session, methods, options));
  }
}
