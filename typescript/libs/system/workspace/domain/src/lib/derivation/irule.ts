import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { IObject } from '../iobject';

export interface IRule<T extends IObject> {
  objectType: Composite;

  roleType: RoleType;

  dependencies: Dependency[];

  derive(match: T): unknown;
}
