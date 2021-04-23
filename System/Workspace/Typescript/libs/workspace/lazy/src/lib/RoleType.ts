import { IAssociationType, IObjectType, IRelationType, IRoleType, Origin } from '@allors/workspace/system';

export class RoleType implements IRoleType {
  associationType: IAssociationType;
  relationType: IRelationType;
  size: number;
  precision: number;
  scale: number;
  isRequired: boolean;
  isUnique: boolean;
  mediaType: string;
  origin: Origin;
  name: string;
  singularName: string;
  pluralName: string;
  objectType: IObjectType;
  isOne: boolean;
  isMany: boolean;
  operandId: string;
}
