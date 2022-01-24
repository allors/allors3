import {
  Component,
  ElementRef,
  ViewEncapsulation,
  Optional,
  ViewChild,
  OnInit,
} from '@angular/core';
import { NgForm } from '@angular/forms';
import { RoleField } from '@allors/base/workspace/angular/foundation';
import * as EasyMDE from 'easymde';

@Component({
  selector: 'a-mat-markdown',
  template: `
    <h4>{{ label }}</h4>
    <textarea #easymde [attr.maxlength]="maxlength"></textarea>
  `,
  encapsulation: ViewEncapsulation.None,
})
export class AllorsMaterialMarkdownComponent
  extends RoleField
  implements OnInit
{
  @ViewChild('easymde', { static: true })
  elementRef: ElementRef;

  easyMDE: EasyMDE;

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  ngOnInit() {
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
