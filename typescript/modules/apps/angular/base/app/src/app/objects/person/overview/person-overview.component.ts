import { Component, Self, AfterViewInit, OnDestroy, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { NavigationActivatedRoute, NavigationService, PanelManagerService, RefreshService } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';
import { Person } from '@allors/workspace/domain/default';

import { M } from '@allors/workspace/meta/default';

@Component({
  templateUrl: './person-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class PersonOverviewComponent implements AfterViewInit, OnDestroy {
  title = 'Person';

  person: Person;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: PanelManagerService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    titleService.setTitle(this.title);
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    this.subscription = combineLatest([this.route.url, this.route.queryParams, this.refreshService.refresh$])
      .pipe(
        switchMap(([, ,]) => {
          const navRoute = new NavigationActivatedRoute(this.route);
          this.panelManager.id = navRoute.id();
          this.panelManager.objectType = m.Person;
          this.panelManager.expanded = navRoute.panel();

          this.panelManager.on();

          const pulls = [
            p.Person({
              objectId: this.panelManager.id,
            }),
          ];

          this.panelManager.onPull(pulls);

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.panelManager.context.reset();
        this.panelManager.onPulled(loaded);

        this.person = loaded.object<Person>(m.Person);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
