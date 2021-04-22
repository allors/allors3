import { IAssociationType, IObjectType, IRelationType, IRoleType, Origin } from '@allors/workspace/system';

export class AssociationType implements IAssociationType {
  relationType: IRelationType;
  roleType: IRoleType;
  origin: Origin;
  name: string;
  singularName: string;
  pluralName: string;
  objectType: IObjectType;
  isOne: boolean;
  isMany: boolean;
  operandId: string;
}
