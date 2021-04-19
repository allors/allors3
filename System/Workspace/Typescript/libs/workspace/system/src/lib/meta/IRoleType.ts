import { IAssociationType } from './IAssociationType';
import { IPropertyType } from './IPropertyType';
import { IRelationType } from './IRelationType';

export interface IRoleType extends IPropertyType {
  associationType: IAssociationType;

  relationType: IRelationType;

  size: number;

  precision: number;

  scale: number;

  isRequired: boolean;

  isUnique: boolean;

  mediaType: string;
}
