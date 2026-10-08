import { PullResult as ConnectionPullResult } from '@allors/system/workspace/connection';
import {
  IObject,
  IPullResult,
  ISession,
  IUnit,
} from '@allors/system/workspace/domain';
import {
  AssociationType,
  Class,
  Interface,
  RoleType,
} from '@allors/system/workspace/meta';
import { Result } from './result';

export class PullResult extends Result implements IPullResult {
  mergeErrors: IObject[];

  private _collections: Map<string, IObject[]>;

  private _objects: Map<string, IObject>;

  constructor(session: ISession, private readonly pulled: ConnectionPullResult) {
    super(session, pulled);
  }

  override get hasErrors(): boolean {
    return super.hasErrors || this.mergeErrors?.length > 0;
  }

  get collections(): Map<string, IObject[]> {
    return (this._collections ??= new Map(
      [...this.pulled.collections].map(([name, ids]) => [
        name,
        ids.map((id) => this.session.instantiate<IObject>(id)),
      ])
    ));
  }

  get objects(): Map<string, IObject> {
    return (this._objects ??= new Map(
      [...this.pulled.objects].map(([name, id]) => [
        name,
        this.session.instantiate<IObject>(id),
      ])
    ));
  }

  get values(): Map<string, IUnit> {
    return this.pulled.values as Map<string, IUnit>;
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
