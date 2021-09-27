import { Component, Input, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';

import { humanize } from '@allors/workspace/meta/system';

import { RoleField } from '../../../../components/forms/role-field';

export interface RadioGroupOption {
  label?: string;
  value: any;
}

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-radio-group',
  templateUrl: './radio-group.component.html',
})
export class AllorsMaterialRadioGroupComponent extends RoleField {
  @Input()
  public options: RadioGroupOption[];

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }

  get keys(): string[] {
    return Object.keys(this.options);
  }

  public optionLabel(option: RadioGroupOption): string {
    return option.label ? option.label : humanize(option.value.toString());
  }

  public optionValue(option: RadioGroupOption): any {
    return option.value;
  }
}
