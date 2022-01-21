import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { Component, Self, AfterViewInit, OnDestroy } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Person } from '@allors/default/workspace/domain';
import {
  AllorsOverviewComponent,
  NavigationActivatedRoute,
  NavigationService,
  PanelManagerService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './person-overview.component.html',
  providers: [PanelManagerService, ContextService],
})
export class PersonOverviewComponent
  extends AllorsOverviewComponent<Person>
  implements AfterViewInit, OnDestroy
{
  private subscription: Subscription;

  constructor(
    @Self() allors: ContextService,
    @Self() panelManager: PanelManagerService,
    titleService: Title,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute
  ) {
    super(allors, panelManager, titleService);
  }

  public ngAfterViewInit(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    this.subscription = combineLatest([
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
    ])
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

        this.object = loaded.object<Person>(m.Person);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
