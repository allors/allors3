import { Component, EventEmitter, Input, Optional, Output } from '@angular/core';
import { NgForm } from '@angular/forms';

import { IObject } from '@allors/workspace/domain/system';

import { RoleField } from '../../../../forms/role-field';

@Component({
  selector: 'a-mat-select',
  templateUrl: './select.component.html',
})
export class AllorsMaterialSelectComponent extends RoleField {
  @Input()
  public display = 'display';

  @Input()
  public options: IObject[];

  @Output()
  public selected: EventEmitter<IObject> = new EventEmitter();

  constructor(@Optional() parentForm: NgForm) {
    super(parentForm);
  }

  public onModelChange(option: IObject): void {
    this.selected.emit(option);
  }

  onRestore(event: Event) {
    event.stopPropagation();
    this.restore();
    this.selected.emit(this.model);
  }
}
