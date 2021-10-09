import { IConfiguration, IInvokeResult, InvokeOptions, IObject, IPullResult, IPushResult, ISession, IWorkspace, Method, Pull } from '@allors/workspace/domain/system';
import { Class, Composite } from '@allors/workspace/meta/system';
import { Observable } from 'rxjs';
import { WorkspaceService } from '../workspace/workspace-service';

export interface Context {
  workspaceService: WorkspaceService;

  session: ISession;

  workspace: IWorkspace;

  configuration: IConfiguration;

  activate(rules: { new (...args: any[]): any }[]): void;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];

  pull(pullOrPulls: Pull | Pull[]): Observable<IPullResult>;

  push(): Observable<IPushResult>;

  invoke(methods: Method | Method[], options?: InvokeOptions): Observable<IInvokeResult>;

  reset(): void;

  hasChanges(): boolean;
}
