import { Component, Input } from '@angular/core';
import { AllorsComponent } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'a-mat-header',
  templateUrl: './header.component.html',
})
export class AllorsMaterialHeaderComponent extends AllorsComponent {
  @Input() title: string;
}
