import { IObject, IPullResult, ISession, IWorkspace, IUnit } from '@allors/workspace/domain/system';
import { PullResponse } from '@allors/protocol/json/system';
import { Result } from '../Result';

export class PullResult extends Result implements IPullResult {
  mergeErrors: IObject[];

  _objects: Map<string, IObject>;

  _collections: Map<string, IObject[]>;

  _values: Map<string, IUnit>;

  workspace: IWorkspace;

  constructor(session: ISession, private pullResponse: PullResponse) {
    super(session, pullResponse);

    this.workspace = session.workspace;
  }

  get hasErrors(): boolean {
    return super.hasErrors || this.mergeErrors.length > 0;
  }

  get collections(): Map<string, IObject[]> {
    return (this._collections ??= new Map(Object.keys(this.pullResponse.c).map((v) => [v.toUpperCase(), this.pullResponse.c[v].map((w) => this.session.instantiate(w))])));
  }

  get objects(): Map<string, IObject> {
    return (this._objects ??= new Map(Object.keys(this.pullResponse.o).map((v) => [v.toUpperCase(), this.session.instantiate(this.pullResponse.o[v])])));
  }

  get values(): Map<string, IUnit> {
    return this._values ?? new Map(Object.keys(this.pullResponse.v).map((v) => [v.toUpperCase(), this.pullResponse.v[v]]));
  }

  collection<T extends IObject>(name: string): T[] {
    return this.collections.get(name.toUpperCase()) as T[];
  }

  object<T extends IObject>(name: string): T {
    return this.objects.get(name.toUpperCase()) as T;
  }

  value(name: string): IUnit | IUnit[] {
    return this.values.get(name.toUpperCase());
  }

  addMergeError(object: IObject) {
    this.mergeErrors ??= [];
    this.mergeErrors.push(object);
  }
}
