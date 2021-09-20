import { Component, Optional, Output, EventEmitter, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { RoleField } from '../../../../components/forms/RoleField';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-datetimepicker',
  styleUrls: ['./datetimepicker.component.scss'],
  templateUrl: './datetimepicker.component.html',
})
export class AllorsMaterialDatetimepickerComponent extends RoleField {
  @Output()
  public selected: EventEmitter<Date> = new EventEmitter();

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }

  get hours(): number | null {
    return this.model?.getHours();
  }

  set hours(value: number | null) {
    if (this.model) {
      const newDate = new Date(this.model);
      newDate.setHours(value);
      this.model = newDate;
    }
  }

  get minutes(): number | null {
    return this.model?.getMinutes();
  }

  set minutes(value: number | null) {
    if (this.model) {
      const newDate = new Date(this.model);
      newDate.setMinutes(value);
      this.model = newDate.toISOString();
    }
  }

  public onModelChange(selected: Date): void {
    this.selected.emit(selected);
  }
}
