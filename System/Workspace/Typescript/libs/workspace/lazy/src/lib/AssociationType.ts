import { IAssociationType, IObjectType, IRoleType, Origin } from '@allors/workspace/system';
import { RelationType } from './RelationType';
import { RoleType } from './RoleType';

export class AssociationType implements IAssociationType {
  readonly relationType: RelationType;
  readonly isMany: boolean;
  operandId: string;
  name: string;
  singularName: string;
  pluralName: string;

  origin: Origin;

  constructor(public roleType: RoleType, public objectType: IObjectType, public isOne: boolean) {
    this.relationType = roleType.relationType;
    this.isMany = !this.isOne;
  }
}
