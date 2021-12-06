import { Component, Input } from '@angular/core';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'a-mat-header',
  templateUrl: './header.component.html',
})
export class AllorsMaterialHeaderComponent {
  @Input() title: string;
}
