import { Component, Optional, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { DateTime } from 'luxon';
import { RoleField } from '../../../../components/forms/role-field';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-datepicker',
  styleUrls: ['./datepicker.component.scss'],
  templateUrl: './datepicker.component.html',
})
export class AllorsMaterialDatepickerComponent extends RoleField {
  @Output()
  selected: EventEmitter<Date> = new EventEmitter();

  private _previousModel: Date;
  private _shadow: DateTime;

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }

  get shadow(): DateTime {
    if (this._previousModel !== this.model) {
      this._shadow = this.model ? DateTime.fromJSDate(this.model) : null;
      this._previousModel = this.model;
    }

    return this._shadow;
  }

  set shadow(value: DateTime) {
    this._shadow = value;
    this.model = value != null ? DateTime.utc(value.year, value.month, value.day, value.hour, value.minute, value.second, value.millisecond).toJSDate() : null;
    this._previousModel = this.model;
  }

  onModelChange(selected: Date): void {
    this.selected.emit(selected);
  }
}
