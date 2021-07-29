import { IObject, IPullResult, ISession, IWorkspace, IUnit } from '@allors/workspace/domain/system';
import { PullResponse } from '@allors/protocol/json/system';
import { Result } from '../Result';

export class PullResult extends Result implements IPullResult {
  objects: Map<string, IObject>;

  collections: Map<string, IObject[]>;

  values: Map<string, IUnit>;

  workspace: IWorkspace;

  constructor(session: ISession, response: PullResponse) {
    super(session, response);

    this.workspace = session.workspace;

    this.objects = new Map(Object.keys(response.o).map((v) => [v.toUpperCase(), session.getOne(response.o[v])]));
    this.collections = new Map(Object.keys(response.c).map((v) => [v.toUpperCase(), response.c[v].map((w) => session.getOne(w))]));
    this.values = new Map(Object.keys(response.v).map((v) => [v.toUpperCase(), response.v[v]]));
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
}
