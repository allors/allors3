import { Component, ViewChild, OnDestroy, OnInit, Self } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { ContextService } from '@allors/workspace/angular/core';
import { Organisation } from '@allors/workspace/domain/default';
import { AllorsMaterialSideNavService, AngularCompositeExtension, AngularMetaPopulationExtension, menu, SideMenuItem } from '@allors/workspace/angular/base';
import { M } from '@allors/workspace/meta/default';

@Component({
  styleUrls: ['main.component.scss'],
  templateUrl: './main.component.html',
  providers: [ContextService],
})
export class MainComponent implements OnInit, OnDestroy {
  selectedInternalOrganisation: Organisation;
  internalOriganisations: Organisation[];

  sideMenuItems: SideMenuItem[] = [];

  private toggleSubscription: Subscription;
  private openSubscription: Subscription;
  private closeSubscription: Subscription;

  @ViewChild('drawer', { static: true }) private sidenav: MatSidenav;

  constructor(@Self() private allors: ContextService, private router: Router, private sideNavService: AllorsMaterialSideNavService) {}

  public ngOnInit(): void {
    const m = this.allors.context.configuration.metaPopulation as M;

    menu(m).forEach((menuItem) => {
      const objectType = menuItem.objectType;

      const sideMenuItem: SideMenuItem = {
        icon: menuItem.icon ?? (objectType?._ as AngularCompositeExtension).icon,
        title: menuItem.title ?? (objectType?._ as AngularCompositeExtension).displayName ?? objectType?.pluralName,
        link: menuItem.link ?? (objectType?._ as AngularCompositeExtension).list,
        children:
          menuItem.children &&
          menuItem.children.map((childMenuItem) => {
            const childObjectType = childMenuItem.objectType;
            return {
              icon: childMenuItem.icon ?? (childObjectType?._ as AngularCompositeExtension).icon,
              title: childMenuItem.title ?? (childObjectType?._ as AngularCompositeExtension).displayName ?? childObjectType?.pluralName,
              link: childMenuItem.link ?? (childObjectType?._ as AngularCompositeExtension).list,
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
