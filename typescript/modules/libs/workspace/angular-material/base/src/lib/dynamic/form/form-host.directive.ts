import { Directive, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[dynamicFormHost]',
})
export class DynamicFormHostDirective {
  constructor(public viewContainerRef: ViewContainerRef) {}
}
