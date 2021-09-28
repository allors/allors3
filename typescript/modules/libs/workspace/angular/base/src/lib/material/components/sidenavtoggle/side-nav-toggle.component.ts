import { Component } from '@angular/core';

import { AllorsMaterialSideNavService } from '../../services/sidenav/side-nav.service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'a-mat-sidenavtoggle',
  templateUrl: './side-nav-toggle.component.html',
})
export class AllorsMaterialSideNavToggleComponent {

  constructor(private sideNavService: AllorsMaterialSideNavService) {
  }

  public toggle() {
    this.sideNavService.toggle();
  }
}
