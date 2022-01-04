import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, Country } from '@allors/workspace/domain/default';
import {
  AllorsPanelDetailComponent,
  PanelService,
  RefreshService,
  SaveService,
  SearchFactory,
  SingletonId,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  selector: 'organisation-overview-detail',
  templateUrl: './organisation-overview-detail.component.html',
  providers: [ContextService, PanelService],
})
export class OrganisationOverviewDetailComponent
  extends AllorsPanelDetailComponent<Organisation>
  implements OnInit, OnDestroy
{
  readonly m: M;

  organisation: Organisation;
  countries: Country[];
  peopleFilter: SearchFactory;

  private subscription: Subscription;

  constructor(
    @Self() allors: ContextService,
    @Self() panel: PanelService,
    public saveService: SaveService,
    public refreshService: RefreshService,
    private singletonId: SingletonId
  ) {
    super(allors, panel);

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Organisation.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const id = this.panel.manager.id;

        pulls.push(
          pull.Organisation({
            name: pullName,
            objectId: id,
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.organisation = loaded.object<Organisation>(pullName);
      }
    };

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.organisation = undefined;

          const id = this.panel.manager.id;

          const pulls = [
            pull.Organisation({
              objectId: id,
              include: {
                Country: x,
                Owner: x,
              },
            }),
            pull.Country({
              sorting: [{ roleType: m.Country.Name }],
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.countries = loaded.collection<Country>(m.Country);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      window.history.back();
    }, this.saveService.errorHandler);
  }
}
