import { Component, Optional, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-slidetoggle',
  templateUrl: './slide-toggle.component.html',
})
export class AllorsMaterialSlideToggleComponent extends RoleField {
  @Output()
  public changed: EventEmitter<any> = new EventEmitter();

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  public change(): void {
    this.changed.emit();
  }
}
