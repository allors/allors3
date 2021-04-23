import { Origin } from '@allors/workspace/system';
import { IAssociationTypeInternals } from './Internals/IAssociationTypeInternals';
import { ICompositeInternals } from './Internals/ICompositeInternals';
import { IRelationTypeInternals } from './Internals/IRelationTypeInternals';
import { IRoleTypeInternals } from './Internals/IRoleTypeInternals';

export class AssociationType implements IAssociationTypeInternals {
  readonly relationType: IRelationTypeInternals;
  readonly operandTag: number;
  readonly origin: Origin;
  readonly isMany: boolean;
  readonly name: string;
  readonly singularName: string;

  private _pluralName: string;

  constructor(public roleType: IRoleTypeInternals, public objectType: ICompositeInternals, public isOne: boolean) {
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
