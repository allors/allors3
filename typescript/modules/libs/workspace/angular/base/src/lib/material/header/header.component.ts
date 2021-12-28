import { Component, Input } from '@angular/core';
import { AllorsComponent } from '../../component';

@Component({
  
  selector: 'a-mat-header',
  templateUrl: './header.component.html',
})
export class AllorsMaterialHeaderComponent extends AllorsComponent {
  @Input() title: string;
}
