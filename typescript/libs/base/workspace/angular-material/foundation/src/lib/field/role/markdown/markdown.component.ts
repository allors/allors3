import {
  Component,
  ElementRef,
  ViewEncapsulation,
  Optional,
  ViewChild,
  OnInit,
  AfterViewInit,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '@allors/base/workspace/angular/foundation';
import * as EasyMDE from 'easymde';

@Component({
  selector: 'a-mat-markdown',
  template: `
    <h4>{{ label }}</h4>
    <textarea #easymde [attr.maxlength]="maxlength" data-allors></textarea>
  `,
  encapsulation: ViewEncapsulation.None,
})
export class AllorsMaterialMarkdownComponent
  extends RoleField
  implements AfterViewInit
{
  @ViewChild('easymde')
  elementRef: ElementRef;

  easyMDE: EasyMDE;

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  override ngAfterViewInit(): void {
    super.ngAfterViewInit();

    this.easyMDE = new EasyMDE({
      element: this.elementRef.nativeElement,
      errorCallback: (errorMessage) => {
        console.log(errorMessage);
      },
    });

    this.easyMDE.value(this.model ?? '');
    this.easyMDE.codemirror.on('change', () => {
      this.model = this.easyMDE.value();
    });

    this.elementRef.nativeElement.easyMDE = this.easyMDE;
  }
}
