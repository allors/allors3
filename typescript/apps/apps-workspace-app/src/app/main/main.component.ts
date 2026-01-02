import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { Component, ViewChild, OnDestroy, OnInit, Self } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';
import { Router, NavigationEnd } from '@angular/router';
import { Organisation } from '@allors/default/workspace/domain';
import {
  ContextService,
  MetaService,
} from '@allors/base/workspace/angular/foundation';
import {
  MenuService,
  NavigationService,
} from '@allors/base/workspace/angular/application';
import {
  AllorsMaterialSideNavService,
  IconService,
  SideMenuItem,
} from '@allors/base/workspace/angular-material/application';

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
    private sideNavService: AllorsMaterialSideNavService,
    private menuService: MenuService,
    private navigation: NavigationService,
    private iconService: IconService,
    private metaService: MetaService
  ) {
    this.allors.context.name = this.constructor.name;
  }

  public ngOnInit(): void {
    this.menuService.menu().forEach((menuItem) => {
      const objectType = menuItem.objectType;

      const sideMenuItem: SideMenuItem = {
        icon: menuItem.icon ?? this.iconService.icon(objectType),
        title: menuItem.title ?? this.metaService.pluralName(objectType),
        link: menuItem.link ?? this.navigation.listUrl(objectType),
        children:
          menuItem.children &&
          menuItem.children.map((childMenuItem) => {
            const childObjectType = childMenuItem.objectType;
            return {
              icon:
                childMenuItem.icon ?? this.iconService.icon(childObjectType),
              title:
                childMenuItem.title ??
                this.metaService.pluralName(childObjectType),
              link:
                childMenuItem.link ?? this.navigation.listUrl(childObjectType),
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
