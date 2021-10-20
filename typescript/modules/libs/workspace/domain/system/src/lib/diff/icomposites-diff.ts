import { IObject } from '../iobject';
import { IDiff } from './idiff';

export interface ICompositesDiff extends IDiff {
  originalRoles: Readonly<IObject[]>;

  changedRoles: Readonly<IObject[]>;
}
