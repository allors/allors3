import { IUnit } from '../types';
import { IDiff } from './idiff';

export interface IUnitDiff extends IDiff {
  originalRole: IUnit;

  changedRole: IUnit;
}
