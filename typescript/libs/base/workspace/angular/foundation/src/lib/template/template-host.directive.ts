import { Directive, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[templateHost]',
})
export class TemplateHostDirective {
  constructor(public viewContainerRef: ViewContainerRef) {}
}
