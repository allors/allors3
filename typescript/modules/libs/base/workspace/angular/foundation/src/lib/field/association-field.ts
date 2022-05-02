// tslint:disable: directive-selector
// tslint:disable: directive-class-suffix
import { Input, Directive, HostBinding } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  AssociationType,
  RoleType,
  assert,
  humanize,
} from '@allors/system/workspace/meta';
import { IObject } from '@allors/system/workspace/domain';

import { Field } from './field';

@Directive()
export abstract class AssociationField extends Field {
  override dataAllorsKind = 'field-association';

  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.id;
  }

  @HostBinding('attr.data-allors-associationtype')
  get dataAllorsAssociationType() {
    return this.associationType?.relationType.tag;
  }

  @Input()
  object: IObject;

  @Input()
  get associationType(): AssociationType {
    return this._associationType;
  }

  set associationType(associationType: AssociationType) {
    assert(
      !associationType || associationType.isOne,
      'AssociationType should have one multiplicity'
    );
    this._associationType = associationType;
  }

  @Input()
  hint: string;

  @Input('label')
  public assignedLabel: string;

  get roleType(): RoleType {
    return this.associationType?.relationType.roleType;
  }

  private id = 0;

  private _associationType: AssociationType;

  constructor(form: NgForm) {
    super(form);
    // TODO: wrap around
    this.id = ++Field.counter;
  }

  get existObject(): boolean {
    return !!this.object;
  }

  get model(): IObject | undefined {
    const model =
      this.existObject && this.associationType
        ? this.object.strategy.getCompositeAssociation(this.associationType)
        : null;

    return model;
  }

  set model(association: IObject | undefined) {
    if (this.existObject) {
      const prevModel = this.model;

      if (prevModel && prevModel !== association) {
        if (this.roleType.isOne) {
          prevModel.strategy.setRole(this.roleType, null);
        } else {
          prevModel.strategy.removeCompositesRole(this.roleType, this.object);
        }
      }

      if (association) {
        if (this.roleType.isOne) {
          association.strategy.setRole(this.roleType, this.object);
        } else {
          association.strategy.addCompositesRole(this.roleType, this.object);
        }
      }
    }
  }

  get name(): string {
    return this.associationType.name + '_' + this.id;
  }

  get label(): string | undefined {
    return this.assignedLabel
      ? this.assignedLabel
      : humanize(this.associationType.name);
  }
}
