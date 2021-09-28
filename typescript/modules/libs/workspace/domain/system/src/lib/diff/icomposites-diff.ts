import { IStrategy } from '../istrategy';
import { IDiff } from './idiff';

export interface ICompositesDiff extends IDiff {
  originalRoles: Readonly<IStrategy[]>;

  changedRoles: Readonly<IStrategy[]>;
}
