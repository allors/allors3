import { IAssociationType, Origin } from '@allors/workspace/system';
import { Composite } from './Composite';
import { RelationType } from './RelationType';
import { RoleType } from './RoleType';

export class AssociationType implements IAssociationType {
  readonly relationType: RelationType;
  readonly operandTag: number;
  readonly origin: Origin;
  readonly isMany: boolean;
  readonly name: string;
  readonly singularName: string;

  private _pluralName: string;

  constructor(public roleType: RoleType, public objectType: Composite, public isOne: boolean) {
    this.relationType = roleType.relationType;
    this.operandTag = this.relationType.tag;
    this.origin = this.relationType.origin;
    this.isMany = !this.isOne;
    this.singularName = this.objectType.singularName + 'Where' + this.roleType.singularName;
    this.name = this.isOne ? this.singularName : this.pluralName;
  }

  get pluralName() {
    return (this._pluralName ??= this.objectType.pluralName + 'Where' + this.roleType.singularName);
  }
}
