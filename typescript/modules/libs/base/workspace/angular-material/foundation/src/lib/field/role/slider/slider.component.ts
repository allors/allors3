import { Component, Input, Optional } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-slider',
  templateUrl: './slider.component.html',
})
export class AllorsMaterialSliderComponent extends RoleField {
  @Input()
  public max: number;

  @Input()
  public min: number;

  @Input()
  public step: number;

  @Input()
  public color: 'primary' | 'accent' | 'warn' = 'accent';

  constructor(@Optional() form: NgForm) {
    super(form);
  }
}
