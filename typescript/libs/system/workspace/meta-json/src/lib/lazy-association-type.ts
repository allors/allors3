import {
  AssociationType,
  Multiplicity,
  Origin,
  RelationType,
  RoleType,
} from '@allors/system/workspace/meta';

import { InternalComposite } from './internal/internal-composite';

export class LazyAssociationType implements AssociationType {
  readonly kind = 'AssociationType';
  readonly _ = {};
  isRoleType = false;
  isAssociationType = true;
  isMethodType = false;

  relationType: RelationType;
  operandTag: string;
  origin: Origin;
  isOne: boolean;
  isMany: boolean;
  name: string;
  singularName: string;

  private _pluralName?: string;

  constructor(
    public roleType: RoleType,
    public objectType: InternalComposite,
    multiplicity: Multiplicity
  ) {
    this.relationType = roleType.relationType;
    this.operandTag = this.relationType.tag;
    this.origin = this.relationType.origin;
    this.isOne = (multiplicity & 2) == 0;
    this.isMany = !this.isOne;
    this.singularName =
      this.objectType.singularName + 'Where' + this.roleType.singularName;
    this.name = this.isOne ? this.singularName : this.pluralName;
  }

  get pluralName() {
    return (this._pluralName ??=
      this.objectType.pluralName + 'Where' + this.roleType.singularName);
  }
}
