import { from, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Context, WorkspaceService } from '@allors/workspace/angular/core';
import { IConfiguration, IInvokeResult, InvokeOptions, IObject, IPullResult, IResult, IRule, ISession, IWorkspace, Method, Pull } from '@allors/workspace/domain/system';
import { Class, Composite } from '@allors/workspace/meta/system';

export class CoreContext implements Context {
  constructor(public workspaceService: WorkspaceService) {
    this.workspace = this.workspaceService.workspace;
    this.configuration = this.workspace.configuration;
    this.session = this.workspace.createSession();

    // Auto activate
    this.session.activate(this.workspace.rules);
  }

  workspace: IWorkspace;

  configuration: IConfiguration;

  session: ISession;

  activate(ruleClasses: (new (...args: any[]) => IRule)[]) {
    if (ruleClasses == null) {
      return;
    }

    const rules = ruleClasses.map((v) => this.workspace.rule(v));
    this.session.activate(rules);
  }

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
    return from(this.session.pull(pulls)).pipe(tap(() => this.session.derive()));
  }

  push(): Observable<IResult> {
    return from(this.session.push());
  }

  invoke(methods: Method | Method[], options?: InvokeOptions): Observable<IInvokeResult> {
    return from(this.session.invoke(methods, options));
  }
}