import { IObject, IPullResult, ISession, IWorkspace, IUnit } from '@allors/workspace/domain/system';
import { PullResponse } from '@allors/protocol/json/system';
import { Result } from '../Result';
import { AssociationType, Class, Interface, RoleType } from '@allors/workspace/meta/system';
import { frozenEmptyMap } from '@allors/workspace/adapters/system';

export class PullResult extends Result implements IPullResult {
  mergeErrors: IObject[];

  _collections: Map<string, IObject[]>;

  _objects: Map<string, IObject>;

  _values: Map<string, IUnit>;

  workspace: IWorkspace;

  constructor(session: ISession, private pullResponse: PullResponse) {
    super(session, pullResponse);

    this.workspace = session.workspace;
  }

  get hasErrors(): boolean {
    return super.hasErrors || this.mergeErrors?.length > 0;
  }

  get collections(): Map<string, IObject[]> {
    return (this._collections ??= this.pullResponse.c ? new Map(Object.keys(this.pullResponse.c).map((v) => [v.toUpperCase(), this.pullResponse.c[v].map((w) => this.session.instantiate(w))])) : (frozenEmptyMap as Map<string, IObject[]>));
  }

  get objects(): Map<string, IObject> {
    return (this._objects ??= this.pullResponse.o ? new Map(Object.keys(this.pullResponse.o).map((v) => [v.toUpperCase(), this.session.instantiate(this.pullResponse.o[v])])) : (frozenEmptyMap as Map<string, IObject>));
  }

  get values(): Map<string, IUnit> {
    return this._values ?? this.pullResponse.v ? new Map(Object.keys(this.pullResponse.v).map((v) => [v.toUpperCase(), this.pullResponse.v[v]])) : (frozenEmptyMap as Map<string, IUnit>);
  }

  collection<T extends IObject>(nameOrClass: string | Class | Interface | AssociationType | RoleType): T[] {
    if (typeof nameOrClass === 'string') {
      return this.collections.get(nameOrClass.toUpperCase()) as T[];
    }

    switch (nameOrClass.kind) {
      case 'AssociationType':
      case 'RoleType':
        return this.collections.get((nameOrClass.isMany ? nameOrClass.pluralName : nameOrClass.singularName).toUpperCase()) as T[];
      default:
        return this.collections.get(nameOrClass.pluralName.toUpperCase()) as T[];
    }
  }

  object<T extends IObject>(nameOrClass: string | Class | Interface): T {
    const name = typeof nameOrClass === 'string' ? nameOrClass : nameOrClass.singularName;
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
