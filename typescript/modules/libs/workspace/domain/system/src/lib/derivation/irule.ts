import { Dependency, RoleType } from '@allors/workspace/meta/system';
import { IObject } from '../iobject';

export interface IRule<T extends IObject> {
  roleType: RoleType;

  dependencies: Dependency[];

  derive(match: T): unknown;
}
