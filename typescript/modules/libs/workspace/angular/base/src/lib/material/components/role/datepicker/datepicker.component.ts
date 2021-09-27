import { Component, Optional, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '../../../../components/forms/role-field';


@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-datepicker',
  styleUrls: ['./datepicker.component.scss'],
  templateUrl: './datepicker.component.html',
})
export class AllorsMaterialDatepickerComponent extends RoleField {
  @Output()
  public selected: EventEmitter<Date> = new EventEmitter();

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }

  public onModelChange(selected: Date): void {
    this.selected.emit(selected);
  }
}
