import { Component, ViewChild, OnDestroy, OnInit, Self } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';

import { ContextService } from '@allors/base/workspace/angular/foundation';
import { Organisation } from '@allors/default/workspace/domain';
import {
  AllorsMaterialSideNavService,
  angularDisplayName,
  angularIcon,
  angularList,
  angularMenu,
  SideMenuItem,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';

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

  constructor(
    @Self() private allors: ContextService,
    private router: Router,
    private sideNavService: AllorsMaterialSideNavService
  ) {
    this.allors.context.name = this.constructor.name;
  }

  public ngOnInit(): void {
    const { context } = this.allors;
    const m = context.configuration.metaPopulation as M;
    angularMenu(m).forEach((menuItem) => {
      const objectType = menuItem.objectType;

      const sideMenuItem: SideMenuItem = {
        icon: menuItem.icon ?? angularIcon(objectType),
        title:
          menuItem.title ??
          angularDisplayName(objectType) ??
          objectType?.pluralName,
        link: menuItem.link ?? angularList(objectType),
        children:
          menuItem.children &&
          menuItem.children.map((childMenuItem) => {
            const childObjectType = childMenuItem.objectType;
            return {
              icon: childMenuItem.icon ?? angularIcon(childObjectType),
              title:
                childMenuItem.title ??
                angularDisplayName(childObjectType) ??
                childObjectType?.pluralName,
              link: childMenuItem.link ?? angularList(childObjectType),
            };
          }),
      };

      this.sideMenuItems.push(sideMenuItem);
    });

    this.router.onSameUrlNavigation = 'reload';
    this.router.events
      .pipe(filter((v) => v instanceof NavigationEnd))
      .subscribe(() => {
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
