import { IObject } from '../iobject';
import { IDiff } from './idiff';

export interface ICompositesDiff extends IDiff {
  originalRoleIds: Readonly<number[]>;

  changedRoleIds: Readonly<number[]>;

  originalRoles: Readonly<IObject[]>;

  changedRoles: Readonly<IObject[]>;
}
