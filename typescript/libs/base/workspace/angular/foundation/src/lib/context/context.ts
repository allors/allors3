import {
  Configuration,
  IInvokeResult,
  InvokeOptions,
  IObject,
  IPullResult,
  IPushResult,
  IRule,
  ISession,
  IWorkspace,
  Method,
  Pull,
} from '@allors/system/workspace/domain';
import { Class, Composite } from '@allors/system/workspace/meta';
import { Observable } from 'rxjs';
import { WorkspaceService } from '../workspace/workspace-service';

export interface Context {
  workspaceService: WorkspaceService;

  session: ISession;

  workspace: IWorkspace;

  configuration: Configuration;

  name: string;

  activate(rules: IRule<IObject>[]): void;

  create<T extends IObject>(cls: Class): T;

  instantiate<T extends IObject>(id: number): T;
  instantiate<T extends IObject>(ids: number[]): T[];
  instantiate<T extends IObject>(obj: T): T;
  instantiate<T extends IObject>(objectType: Composite): T[];

  pull(pullOrPulls: Pull | Pull[]): Observable<IPullResult>;

  push(): Observable<IPushResult>;

  invoke(
    methods: Method | Method[],
    options?: InvokeOptions
  ): Observable<IInvokeResult>;

  reset(): void;

  hasChanges(): boolean;
}
