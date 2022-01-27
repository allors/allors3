import { take } from 'rxjs/operators';
import { Component, Optional, ViewChild, NgZone } from '@angular/core';
import { NgForm } from '@angular/forms';
import { CdkTextareaAutosize } from '@angular/cdk/text-field';
import { RoleField } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-textarea',
  templateUrl: './textarea.component.html',
})
export class AllorsMaterialTextareaComponent extends RoleField {
  @ViewChild('autosize') autosize: CdkTextareaAutosize;

  constructor(@Optional() form: NgForm, private ngZone: NgZone) {
    super(form);
  }

  triggerResize() {
    // Wait for changes to be applied, then trigger textarea resize.
    this.ngZone.onStable
      .pipe(take(1))
      .subscribe(() => this.autosize.resizeToFitContent(true));
  }
}
