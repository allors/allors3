import { Input, Component } from '@angular/core';
import { Router } from '@angular/router';
import { AllorsComponent } from '@allors/base/workspace/angular/foundation';
import { SideMenuItem } from './side-menu-item';

@Component({
  selector: 'a-mat-sidemenu',
  styleUrls: ['side-menu.component.scss'],
  templateUrl: './side-menu.component.html',
})
export class AllorsMaterialSideMenuComponent extends AllorsComponent {
  @Input()
  public items: SideMenuItem[];

  constructor(public router: Router) {
    super();
  }

  hasChildren(item: SideMenuItem): boolean {
    return this.children(item)?.length > 0 ?? false;
  }

  children(parent: SideMenuItem): SideMenuItem[] {
    return parent.children?.filter((v) => v.children || v.link);
  }
}
