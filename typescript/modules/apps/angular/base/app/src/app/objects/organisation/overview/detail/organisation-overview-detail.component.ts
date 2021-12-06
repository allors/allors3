import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Organisation, Country } from '@allors/workspace/domain/default';
import { PanelService, RefreshService, SaveService, SingletonId } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'organisation-overview-detail',
  templateUrl: './organisation-overview-detail.component.html',
  providers: [ContextService, PanelService],
})
export class OrganisationOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  organisation: Organisation;
  countries: Country[];

  private subscription: Subscription;

  constructor(@Self() public allors: ContextService, @Self() public panel: PanelService, public saveService: SaveService, public refreshService: RefreshService, private singletonId: SingletonId) {
    this.allors.context.name = this.constructor.name;
    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Organisation Details';
    panel.icon = 'business';
    panel.expandable = true;

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
