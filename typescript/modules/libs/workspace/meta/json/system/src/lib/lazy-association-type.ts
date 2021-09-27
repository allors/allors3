import { Multiplicity, Origin } from '@allors/workspace/meta/system';

import { InternalAssociationType } from './internal/internal-association-type';
import { InternalComposite } from './internal/internal-composite';
import { InternalRelationType } from './internal/internal-relation-type';
import { InternalRoleType } from './internal/internal-role-type';

export class LazyAssociationType implements InternalAssociationType {
  readonly kind = 'AssociationType';
  readonly isRoleType = false;
  readonly isAssociationType = true;
  readonly isMethodType = false;

  readonly relationType: InternalRelationType;
  readonly operandTag: string;
  readonly origin: Origin;
  readonly isOne: boolean;
  readonly isMany: boolean;
  readonly name: string;
  readonly singularName: string;

  private _pluralName?: string;

  constructor(public roleType: InternalRoleType, public objectType: InternalComposite, multiplicity: Multiplicity) {
    this.relationType = roleType.relationType;
    this.operandTag = this.relationType.tag;
    this.origin = this.relationType.origin;
    this.isOne = (multiplicity & 2) == 0;
    this.isMany = !this.isOne;
    this.singularName = this.objectType.singularName + 'Where' + this.roleType.singularName;
    this.name = this.isOne ? this.singularName : this.pluralName;
  }

  get pluralName() {
    return (this._pluralName ??= this.objectType.pluralName + 'Where' + this.roleType.singularName);
  }
}
