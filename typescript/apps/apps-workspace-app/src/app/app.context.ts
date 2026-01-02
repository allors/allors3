import { from, Observable } from 'rxjs';

import {
  Context,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  Configuration,
  IInvokeResult,
  InvokeOptions,
  IObject,
  IPullResult,
  IResult,
  IRule,
  ISession,
  IWorkspace,
  Method,
  Pull,
} from '@allors/system/workspace/domain';
import { Class, Composite } from '@allors/system/workspace/meta';

export class AppContext implements Context {
  constructor(public workspaceService: WorkspaceService) {
    this.workspace = this.workspaceService.workspace;
    this.configuration = this.workspace.configuration;
    this.session = this.workspace.createSession();

    // Auto activate
    const rules = this.workspace.configuration.rules;
    this.session.activate(rules);
  }

  workspace: IWorkspace;

  configuration: Configuration;

  session: ISession;

  get name(): string {
    return this.session.context;
  }

  set name(value: string) {
    this.session.context = value;
  }

  activate(rules: IRule<IObject>[]) {
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
    return from(this.session.pull(pulls));
  }

  push(): Observable<IResult> {
    return from(this.session.push());
  }

  invoke(
    methods: Method | Method[],
    options?: InvokeOptions
  ): Observable<IInvokeResult> {
    return from(this.session.invoke(methods, options));
  }
}
