import { Component, Optional, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { DateTime } from 'luxon';
import { RoleField } from '@allors/base/workspace/angular/foundation';
import { MatDatepickerInputEvent } from '@angular/material/datepicker';
import { DateAdapter, MAT_DATE_LOCALE } from '@angular/material/core';
import { Platform } from '@angular/cdk/platform';
import { LuxonDateAdapter, MatLuxonDateAdapterOptions } from '@angular/material-luxon-adapter';

export class MonthpickerDateAdapter extends LuxonDateAdapter {
  constructor(matDateLocale: string, options: MatLuxonDateAdapterOptions) {
    super(matDateLocale, options);
  }

  override parse(value: any, parseFormat: string | string[]): DateTime | null {
    return super.parse(`1/${value}`, parseFormat);
  }

  override format(date: DateTime, displayFormat: string): string {
    return `${date.month}/${date.year}`;
  }
}

@Component({
  selector: 'a-mat-monthpicker',
  styleUrls: ['./monthpicker.component.scss'],
  templateUrl: './monthpicker.component.html',
  providers: [
    {
      provide: DateAdapter,
      useClass: MonthpickerDateAdapter,
      deps: [MAT_DATE_LOCALE, Platform],
    },
  ],
})
export class AllorsMaterialMonthpickerComponent extends RoleField {
  @Output()
  selected: EventEmitter<Date> = new EventEmitter();

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
    this.model =
      value != null
        ? DateTime.utc(
            value.year,
            value.month,
            value.day,
            value.hour,
            value.minute,
            value.second,
            value.millisecond
          ).toJSDate()
        : null;
    this._previousModel = this.model;
  }

  onModelChange(selected: Date): void {
    this.selected.emit(selected);
  }

  public emitDateChange(event: MatDatepickerInputEvent<Date | null, unknown>): void {
    this.selected.emit(event.value);
  }

  public monthChanged(value: any, widget: any): void {
    this.shadow = value;
    widget.close();
  }
}
