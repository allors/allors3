import { Component } from '@angular/core';
import { AllorsComponent } from '../../component';

import { AllorsMaterialSideNavService } from '../sidenav/side-nav.service';

@Component({
  selector: 'a-mat-sidenavtoggle',
  templateUrl: './side-nav-toggle.component.html',
})
export class AllorsMaterialSideNavToggleComponent extends AllorsComponent {
  constructor(private sideNavService: AllorsMaterialSideNavService) {
    super();
  }

  public toggle() {
    this.sideNavService.toggle();
  }
}
