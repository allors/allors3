import { Component, Optional, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { DateTime } from 'luxon';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-datetimepicker',
  styleUrls: ['./datetimepicker.component.scss'],
  templateUrl: './datetimepicker.component.html',
})
export class AllorsMaterialDatetimepickerComponent extends RoleField {
  @Output()
  public selected: EventEmitter<Date> = new EventEmitter();

  private _previousModel: Date;
  private _shadow: DateTime;

  constructor(@Optional() form: NgForm) {
    super(form);
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
    this.model = value?.toJSDate();
    this._previousModel = this.model;
  }

  get hour(): number | null {
    return this.shadow?.hour;
  }

  set hour(value: number | null) {
    if (this.shadow) {
      this.shadow = this.shadow.set({ hour: value ?? 0 });
    }
  }

  get minute(): number | null {
    return this.shadow?.minute;
  }

  set minute(value: number | null) {
    if (this.shadow) {
      this.shadow = this.shadow.set({ minute: value ?? 0 });
    }
  }

  public onModelChange(selected: Date): void {
    this.selected.emit(selected);
  }
}
