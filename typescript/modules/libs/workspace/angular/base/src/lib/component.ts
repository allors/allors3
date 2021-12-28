import { HostBinding, Directive } from '@angular/core';

@Directive()
export abstract class AllorsComponent {
  @HostBinding('attr.data-allors-component')
  dataAllorsComponent = this.constructor.name;
}
