import {
  IObject,
  IPullResult,
  ISession,
  IWorkspace,
  IUnit,
} from '@allors/system/workspace/domain';
import { PullResponse } from '@allors/system/common/protocol-json';
import { Result } from '../result';
import {
  AssociationType,
  Class,
  Interface,
  RoleType,
} from '@allors/system/workspace/meta';
import { frozenEmptyMap } from '@allors/system/workspace/adapters';

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

  override get hasErrors(): boolean {
    return super.hasErrors || this.mergeErrors?.length > 0;
  }

  get collections(): Map<string, IObject[]> {
    return (this._collections ??= this.pullResponse.c
      ? new Map(
          Object.keys(this.pullResponse.c).map((v) => [
            v.toUpperCase(),
            this.pullResponse.c[v].map((w) => this.session.instantiate(w)),
          ])
        )
      : (frozenEmptyMap as Map<string, IObject[]>));
  }

  get objects(): Map<string, IObject> {
    return (this._objects ??= this.pullResponse.o
      ? new Map(
          Object.keys(this.pullResponse.o).map((v) => [
            v.toUpperCase(),
            this.session.instantiate(this.pullResponse.o[v]),
          ])
        )
      : (frozenEmptyMap as Map<string, IObject>));
  }

  get values(): Map<string, IUnit> {
    return this._values ?? this.pullResponse.v
      ? new Map(
          Object.keys(this.pullResponse.v).map((v) => [
            v.toUpperCase(),
            this.pullResponse.v[v],
          ])
        )
      : (frozenEmptyMap as Map<string, IUnit>);
  }

  collection<T extends IObject>(
    nameOrType: string | Class | Interface | AssociationType | RoleType
  ): T[] {
    if (typeof nameOrType === 'string') {
      return (this.collections.get(nameOrType.toUpperCase()) as T[]) ?? [];
    }

    switch (nameOrType.kind) {
      case 'AssociationType':
      case 'RoleType':
        return (
          (this.collections.get(
            (nameOrType.isMany
              ? nameOrType.pluralName
              : nameOrType.singularName
            ).toUpperCase()
          ) as T[]) ?? []
        );
      default:
        return (
          (this.collections.get(nameOrType.pluralName.toUpperCase()) as T[]) ??
          []
        );
    }
  }

  object<T extends IObject>(
    nameOrType: string | Class | Interface | AssociationType | RoleType
  ): T {
    const name =
      typeof nameOrType === 'string' ? nameOrType : nameOrType.singularName;
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
