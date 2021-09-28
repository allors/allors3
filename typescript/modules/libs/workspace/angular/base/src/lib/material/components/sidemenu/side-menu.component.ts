import { Input, Component } from '@angular/core';
import { Router } from '@angular/router';

import { SideMenuItem } from './side-menu-item';

@Component({
  selector: 'a-mat-sidemenu',
  styleUrls: ['side-menu.component.scss'],
  templateUrl: './side-menu.component.html',
})
export class AllorsMaterialSideMenuComponent {

  @Input()
  public items: SideMenuItem[];

  constructor(public router: Router) { }

  public hasChildren(item: SideMenuItem): boolean {
    if (item.children) {
      return item.children.length > 0 ?? false;
    }

    return false;
  }
}
