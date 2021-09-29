import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { InternalOrganisation, Locale, Organisation, Currency, CustomOrganisationClassification, IndustryClassification, LegalForm } from '@allors/workspace/domain/default';
import { PanelService, RefreshService, SaveService, TestScope, SingletonId } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'organisation-overview-detail',
  templateUrl: './organisation-overview-detail.component.html',
  providers: [SessionService, PanelService],
})
export class OrganisationOverviewDetailComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  organisation: Organisation;
  locales: Locale[];
  classifications: CustomOrganisationClassification[];
  industries: IndustryClassification[];
  internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  legalForms: LegalForm[];
  currencies: Currency[];

  constructor(
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,

    public saveService: SaveService,
    public refreshService: RefreshService,
    private singletonId: SingletonId,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Organisation Details';
    panel.icon = 'business';
    panel.expandable = true;

    // Collapsed
    const pullName = `${this.panel.name}_${this.m.Organisation.tag}`;

    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const { pullBuilder: pull } = this.m;
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
    // Expanded
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.organisation = undefined;

          const { m, x, pull } = this.metaService;
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
            pull.Singleton({
              object: this.singletonId.value,
              select: {
                Locales: {
                  include: {
                    Language: x,
                    Country: x,
                  },
                },
              },
            }),
            pull.Organisation({
              objectId: id,
              include: {
                LogoImage: x,
              },
            }),
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.Name }],
            }),
            pull.CustomOrganisationClassification({
              sorting: [{ roleType: m.CustomOrganisationClassification.Name }],
            }),
            pull.IndustryClassification({
              sorting: [{ roleType: m.IndustryClassification.Name }],
            }),
            pull.LegalForm({
              sorting: [{ roleType: m.LegalForm.Description }],
            }),
          ];

          return this.allors.client.pullReactive(this.allors.session, pulls);
        })
      )
      .subscribe((loaded) => {
        this.organisation = loaded.object<Organisation>(m.Organisation);
        this.internalOrganisation = loaded.object<Organisation>(m.InternalOrganisation);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Locale) || [];
        this.classifications = loaded.collection<CustomOrganisationClassification>(m.CustomOrganisationClassification);
        this.industries = loaded.collection<IndustryClassification>(m.IndustryClassification);
        this.legalForms = loaded.collection<LegalForm>(m.LegalForm);
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.client.pushReactive(this.allors.session).subscribe(() => {
      this.refreshService.refresh();
      window.history.back();
    }, this.saveService.errorHandler);
  }
}
