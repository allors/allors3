import { Component, ViewChild, OnDestroy, OnInit, Self } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { SessionService } from '@allors/workspace/angular/core';

import { menu } from './main.menu';
import { Organisation } from '@allors/workspace/domain/default';
import { AllorsMaterialSideNavService, SideMenuItem } from '@allors/workspace/angular/base';
import { Composite } from '@allors/workspace/meta/system';
import { M } from '@allors/workspace/meta/default';

@Component({
  styleUrls: ['main.component.scss'],
  templateUrl: './main.component.html',
  providers: [SessionService],
})
export class MainComponent implements OnInit, OnDestroy {
  selectedInternalOrganisation: Organisation;
  internalOriganisations: Organisation[];

  sideMenuItems: SideMenuItem[] = [];

  private toggleSubscription: Subscription;
  private openSubscription: Subscription;
  private closeSubscription: Subscription;

  @ViewChild('drawer', { static: true }) private sidenav: MatSidenav;

  constructor(@Self() private allors: SessionService, private router: Router, private sideNavService: AllorsMaterialSideNavService) {}

  public ngOnInit(): void {
    const { workspace } = this.allors;
    const m = workspace.configuration.metaPopulation as M;
    const angularMeta = workspace.services.angularMetaService;

    menu.forEach((menuItem) => {
      const objectType = menuItem.tag ? (m.metaObjectByTag.get(menuItem.tag) as Composite) : null;

      const sideMenuItem: SideMenuItem = {
        icon: menuItem.icon ?? angularMeta.for(objectType)?.icon,
        title: menuItem.title ?? angularMeta.for(objectType)?.displayName ?? objectType.pluralName,
        link: menuItem.link ?? angularMeta.for(objectType)?.list,
        children:
          menuItem.children &&
          menuItem.children.map((childMenuItem) => {
            const childObjectType = childMenuItem.tag ? (m.metaObjectByTag.get(childMenuItem.tag) as Composite) : null;
            return {
              icon: childMenuItem.icon ?? angularMeta.for(childObjectType)?.icon,
              title: childMenuItem.title ?? angularMeta.for(childObjectType)?.displayName ?? childObjectType.pluralName,
              link: childMenuItem.link ?? angularMeta.for(childObjectType)?.list,
            };
          }),
      };

      this.sideMenuItems.push(sideMenuItem);
    });

    this.router.onSameUrlNavigation = 'reload';
    this.router.events.pipe(filter((v) => v instanceof NavigationEnd)).subscribe(() => {
      if (this.sidenav) {
        this.sidenav.close();
      }
    });

    this.toggleSubscription = this.sideNavService.toggle$.subscribe(() => {
      this.sidenav.toggle();
    });

    this.openSubscription = this.sideNavService.open$.subscribe(() => {
      this.sidenav.open();
    });

    this.closeSubscription = this.sideNavService.close$.subscribe(() => {
      this.sidenav.close();
    });
  }

  ngOnDestroy(): void {
    this.toggleSubscription.unsubscribe();
    this.openSubscription.unsubscribe();
    this.closeSubscription.unsubscribe();
  }

  public toggle() {
    this.sideNavService.toggle();
  }
}
