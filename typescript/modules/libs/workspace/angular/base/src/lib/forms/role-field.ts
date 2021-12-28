// tslint:disable: directive-selector
// tslint:disable: directive-class-suffix
import { AfterViewInit, Input, OnDestroy, QueryList, ViewChildren, Directive, HostBinding } from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';

import { RoleType, humanize, UnitTags } from '@allors/workspace/meta/system';
import { IObject } from '@allors/workspace/domain/system';

import { Field } from './field';

@Directive()
export abstract class RoleField extends Field implements AfterViewInit, OnDestroy {
  @HostBinding('attr.data-allors-id')
  get dataAllorsId() {
    return this.object?.id;
  }

  @HostBinding('attr.data-allors-roletype')
  get dataAllorsRoleType() {
    return this.roleType?.relationType.tag;
  }

  @Input()
  public object: IObject;

  @Input()
  public roleType: RoleType;

  @Input()
  assignedRoleType: RoleType;

  @Input()
  derivedInitialRole: IObject;

  // tslint:disable-next-line:no-input-rename
  @Input('name')
  public assignedName: string;

  // tslint:disable-next-line:no-input-rename
  @Input('disabled')
  public assignedDisabled: boolean;

  // tslint:disable-next-line:no-input-rename
  @Input('required')
  public assignedRequired: boolean;

  // tslint:disable-next-line:no-input-rename
  @Input('label')
  public assignedLabel: string;

  @Input()
  public readonly: boolean;

  @Input()
  public hint: string;

  @Input()
  public focus: boolean;

  @Input()
  public emptyStringIsNull = true;

  @ViewChildren(NgModel) private controls: QueryList<NgModel>;

  private id = 0;

  constructor(private parentForm: NgForm) {
    super();
    // TODO: wrap around
    this.id = ++Field.counter;
  }

  get ExistObject(): boolean {
    return !!this.object;
  }

  get model(): any {
    if (this.ExistObject) {
      if (this.assignedRoleType) {
        if (this.object.strategy.hasChanged(this.assignedRoleType)) {
          return this.object.strategy.getRole(this.assignedRoleType);
        }

        if (this.object.strategy.isNew && this.derivedInitialRole) {
          return this.derivedInitialRole;
        }
      }

      return this.object.strategy.getRole(this.roleType);
    }

    return undefined;
  }

  set model(value: any) {
    if (this.ExistObject) {
      if (this.emptyStringIsNull && value === '') {
        value = null;
      }

      if (this.roleType.objectType.tag === UnitTags.Integer) {
        if (value != null && !Number.isInteger(value)) {
          try {
            value = Number.parseInt(value);
          } catch {
            value = null;
          }
        }
      }

      if (this.roleType.objectType.tag === UnitTags.Decimal) {
        value = (value as string)?.replace(',', '.');
      }

      if (this.assignedRoleType) {
        if (this.object.strategy.isNew && this.derivedInitialRole && this.derivedInitialRole === value) {
          this.object.strategy.setRole(this.assignedRoleType, null);
        } else {
          this.object.strategy.setRole(this.assignedRoleType, value);
        }
      } else {
        this.object.strategy.setRole(this.roleType, value);
      }
    }
  }

  get canRead(): boolean | undefined {
    return this.object?.strategy.canRead(this.roleType);
  }

  get canWrite(): boolean | undefined {
    return this.object?.strategy.canWrite(this.assignedRoleType ?? this.roleType);
  }

  get textType(): string {
    if (this.roleType.objectType.tag == UnitTags.Integer || this.roleType.objectType.tag == UnitTags.Float) {
      return 'number';
    }

    return 'text';
  }

  get pattern(): string {
    if (this.roleType.objectType.tag == UnitTags.Decimal) {
      return '^\\d*(\\.\\d+)?$';
    }

    return null;
  }

  get maxlength(): number {
    if (this.roleType.objectType.tag == UnitTags.String) {
      return this.roleType.size !== -1 ? this.roleType.size ?? 256 : null;
    }

    return null;
  }

  get name(): string {
    return this.assignedName ? this.assignedName : this.roleType.name + '_' + this.id;
  }

  get label(): string {
    return this.assignedLabel ? this.assignedLabel : humanize(this.roleType.name);
  }

  get required(): boolean {
    if (this.assignedRequired) {
      return this.assignedRequired;
    }

    if (this.object) {
      return this.object.strategy.cls.requiredRoleTypes.has(this.roleType);
    }

    return false;
  }

  get disabled(): boolean {
    return !this.canWrite || !!this.assignedDisabled;
  }

  get canRestore(): boolean {
    return this.ExistObject && this.assignedRoleType && this.object.strategy.hasChanged(this.assignedRoleType);
  }

  restore(): void {
    this.ExistObject && this.object?.strategy.restoreRole(this.assignedRoleType);
  }

  public add(value: IObject) {
    if (this.ExistObject) {
      this.object.strategy.addCompositesRole(this.roleType, value);
    }
  }

  public remove(value: IObject) {
    if (this.ExistObject) {
      this.object.strategy.removeCompositesRole(this.roleType, value);
    }
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
