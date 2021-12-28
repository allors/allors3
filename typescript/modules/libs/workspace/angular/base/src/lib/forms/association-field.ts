// tslint:disable: directive-selector
// tslint:disable: directive-class-suffix
import { AfterViewInit, Input, OnDestroy, QueryList, ViewChildren, Directive, HostBinding } from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';

import { AssociationType, RoleType, assert, humanize } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

import { Field } from './field';

@Directive()
export abstract class AssociationField extends Field implements AfterViewInit, OnDestroy {
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.id;
  }

  @HostBinding('attr.data-allors-roletype')
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
    assert(!associationType || associationType.isOne, 'AssociationType should have one multiplicity');
    this._associationType = associationType;
  }

  @Input()
  hint: string;

  // tslint:disable-next-line:no-input-rename
  @Input('label')
  public assignedLabel: string;

  @ViewChildren(NgModel) private controls: QueryList<NgModel>;

  get roleType(): RoleType {
    return this.associationType?.relationType.roleType;
  }

  private id = 0;

  private _associationType: AssociationType;

  constructor(private parentForm: NgForm) {
    super();
    // TODO: wrap around
    this.id = ++Field.counter;
  }

  get existObject(): boolean {
    return !!this.object;
  }

  get model(): IObject | undefined {
    const model = this.existObject && this.associationType ? this.object.strategy.getCompositeAssociation(this.associationType) : null;

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
    return this.assignedLabel ? this.assignedLabel : humanize(this.associationType.name);
  }

  public ngAfterViewInit(): void {
    if (this.parentForm) {
      this.controls.forEach((control: NgModel) => {
        this.parentForm.addControl(control);
      });
    }
  }

  public ngOnDestroy(): void {
    if (this.parentForm) {
      this.controls.forEach((control: NgModel) => {
        this.parentForm.removeControl(control);
      });
    }
  }
}
