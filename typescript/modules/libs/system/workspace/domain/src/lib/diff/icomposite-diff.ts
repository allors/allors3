import { IObject } from '../iobject';
import { IDiff } from './idiff';

export interface ICompositeDiff extends IDiff {
  originalRoleId: number;

  changedRoleId: number;

  originalRole: IObject;

  changedRole: IObject;
}
