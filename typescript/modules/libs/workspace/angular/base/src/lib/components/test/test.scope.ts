import { HostBinding, Directive } from '@angular/core';

@Directive()
export abstract class TestScope {
  @HostBinding('attr.data-allors-component-type')
  dataAllorsComponentType = this.constructor.name;
}
