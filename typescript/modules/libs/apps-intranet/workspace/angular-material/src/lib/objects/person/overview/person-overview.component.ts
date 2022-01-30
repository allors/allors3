import { Component, Self, OnDestroy, Injector, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { M } from '@allors/default/workspace/meta';
import { Person, Employment } from '@allors/default/workspace/domain';
import {
  NavigationActivatedRoute,
  NavigationService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import { OldPanelManagerService } from '@allors/base/workspace/angular/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';

@Component({
  templateUrl: './person-overview.component.html',
  providers: [OldPanelManagerService, ContextService],
})
export class PersonOverviewComponent implements OnInit, OnDestroy {
  title = 'Person';

  person: Person;
  employee: boolean;

  subscription: Subscription;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    @Self() public panelManager: OldPanelManagerService,
    public refreshService: RefreshService,
    public navigation: NavigationService,
    private route: ActivatedRoute,
    public injector: Injector,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    titleService.setTitle(this.title);
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    this.subscription = combineLatest(
      this.route.url,
      this.route.queryParams,
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
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
            p.Person({
              objectId: this.panelManager.id,
              select: {
                EmploymentsWhereEmployee: {},
              },
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
        const employments = loaded.collection<Employment>(
          m.Person.EmploymentsWhereEmployee
        );
        this.employee = employments?.length > 0;
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
