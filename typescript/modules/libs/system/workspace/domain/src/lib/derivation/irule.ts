import { Composite, Dependency, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';

export interface IRule<T extends IObject> {
  objectType: Composite;

  roleType: RoleType;

  dependencies: Dependency[];

  derive(match: T): unknown;
}
