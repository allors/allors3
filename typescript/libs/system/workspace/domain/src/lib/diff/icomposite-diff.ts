import { IObject } from '../iobject';
import { IDiff } from './idiff';

export interface ICompositeDiff extends IDiff {
  originalRole: IObject;

  changedRole: IObject;
}
