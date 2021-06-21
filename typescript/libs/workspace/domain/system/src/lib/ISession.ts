import { Observable } from "rxjs";
import { Composite,  Class} from "@allors/workspace/meta/system";
import { IObject } from './IObject';
import { IWorkspace } from './IWorkspace';
import { Method } from './operands/Method';
import { InvokeOptions } from './database/InvokeOptions';
import { IInvokeResult } from './database/IInvokeResult';
import { IPullResult } from './database/IPullResult';
import { IPushResult } from './database/IPushResult';
import { ISessionServices } from './state/ISessionServices';
import { Pull } from './data/Pull';
import { Procedure } from './data/Procedure';
import { IChangeSet } from './IChangeSet';

export interface ISession {
  workspace: IWorkspace;

  state: ISessionServices;

  create(cls: Class): IObject;

  getOne<T extends IObject>(obj: IObject |  number | string): T;

  getMany<T extends IObject>(objects: IObject[] | number[] | string[]): Generator<T, void, unknown>;

  getAll<T extends IObject>(objectType?: Composite): Generator<T, void, unknown>;

  invoke(method: Method | Method[], options?: InvokeOptions): Observable<IInvokeResult>;

  pull(pullsOrProceudre: Pull[] | Procedure, pulls?: Pull[]): Observable<IPullResult>;

  push(): Observable<IPushResult>;

  checkpoint(): IChangeSet;
}
