import { IPropertyType } from './IPropertyType';
import { IRelationType } from './IRelationType';
import { IRoleType } from './IRoleType';

export interface IAssociationType extends IPropertyType {
  relationType: IRelationType;

  roleType: IRoleType;
}
