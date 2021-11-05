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

  constructor(public router: Router) {}

  hasChildren(item: SideMenuItem): boolean {
    return this.children(item)?.length > 0 ?? false;
  }

  children(parent: SideMenuItem): SideMenuItem[] {
    return parent.children?.filter((v) => v.children || v.link);
  }
}
