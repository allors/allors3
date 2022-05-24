import { Component, Input, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';
import { humanize } from '@allors/system/workspace/meta';
import { RoleField } from '@allors/base/workspace/angular/foundation';
import { IObject, IUnit } from '@allors/system/workspace/domain';

export interface RadioGroupOption {
  label?: string;
  value: IUnit | IObject;
}

function isObject(obj: any | IObject): obj is IObject {
  return (obj as IObject).strategy !== undefined;
}

@Component({
  selector: 'a-mat-radio-group',
  templateUrl: './radio-group.component.html',
})
export class AllorsMaterialRadioGroupComponent extends RoleField {
  @Input()
  public display = 'display';

  @Input()
  public options: (RadioGroupOption | IObject)[];

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  get keys(): string[] {
    return Object.keys(this.options);
  }

  public optionLabel(option: RadioGroupOption | IObject): string {
    if (isObject(option)) {
      return option[this.display];
    } else {
      if (option.label) {
        return option.label;
      }

      if (isObject(option.value)) {
        return option.value[this.display];
      }

      return humanize(option.value.toString());
    }
  }

  public optionValue(option: RadioGroupOption | IObject): any {
    if (isObject(option)) {
      return option;
    } else {
      return option.value;
    }
  }

  public dataValue(option: RadioGroupOption | IObject): any {
    if (isObject(option)) {
      return option.id;
    } else {
      if (isObject(option.value)) {
        return option.value.id;
      } else {
        return option.value;
      }
    }
  }
}
