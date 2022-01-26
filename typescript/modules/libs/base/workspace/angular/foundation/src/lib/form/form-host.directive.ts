import { Directive, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[formHost]',
})
export class FormHostDirective {
  constructor(public viewContainerRef: ViewContainerRef) {}
}
