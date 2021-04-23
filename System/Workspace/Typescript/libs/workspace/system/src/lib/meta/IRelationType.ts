import { Multiplicity } from '../Multiplicity';
import { IAssociationType } from './IAssociationType';
import { IMetaObject } from './IMetaObject';
import { IRoleType } from './IRoleType';

export interface IRelationType extends IMetaObject {
  associationType: IAssociationType;

  roleType: IRoleType;

  multiplicity: Multiplicity;

  isDerived: boolean;
}
