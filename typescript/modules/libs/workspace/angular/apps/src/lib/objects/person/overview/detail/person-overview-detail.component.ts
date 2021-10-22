import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Currency, Enumeration, GenderType, InternalOrganisation, Locale, Organisation, Person, Salutation } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService, SingletonId, TestScope } from '@allors/workspace/angular/base';
import { ContextService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'person-overview-detail',
  templateUrl: './person-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class PersonOverviewDetailComponent extends TestScope implements OnInit, OnDestroy {
  readonly m: M;

  person: Person;

  internalOrganisation: InternalOrganisation;
  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];

  private subscription: Subscription;
  currencies: Currency[];

  constructor(
    @Self() public allors: ContextService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private singletonId: SingletonId,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.context.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Personal Data';
    panel.icon = 'person';
    panel.expandable = true;

    // Minimized
    const pullName = `${this.panel.name}_${this.m.Person.tag}`;

    panel.onPull = (pulls) => {
      this.person = undefined;

      if (this.panel.isCollapsed) {
        const m = this.m; const { pullBuilder: pull } = m;
        const x = {};
        const id = this.panel.manager.id;

        pulls.push(
          pull.Person({
            name: pullName,
            objectId: id,
            include: {
              GeneralEmail: x,
              PersonalEmailAddress: x,
            },
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.person = loaded.object<Person>(pullName);
      }
    };
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    // Maximized
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.person = undefined;
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Singleton({
              objectId: this.singletonId.value,
              select: {
                Locales: {
                  include: {
                    Language: x,
                    Country: x,
                  },
                },
              },
            }),
            pull.Currency({
              predicate: { kind: 'Equals', propertyType: m.Currency.IsActive, value: true },
              sorting: [{ roleType: m.Currency.Name }],
            }),
            pull.GenderType({
              predicate: { kind: 'Equals', propertyType: m.GenderType.IsActive, value: true },
              sorting: [{ roleType: m.GenderType.Name }],
            }),
            pull.Salutation({
              predicate: { kind: 'Equals', propertyType: m.Salutation.IsActive, value: true },
              sorting: [{ roleType: m.Salutation.Name }],
            }),
            pull.Person({
              objectId: id,
              include: {
                PreferredCurrency: x,
                Gender: x,
                Salutation: x,
                Locale: x,
                Picture: x,
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.person = loaded.object<Person>(m.Person);
        this.internalOrganisation = this.fetcher.getInternalOrganisation(loaded);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Singleton.Locales) || [];
        this.genders = loaded.collection<GenderType>(m.GenderType);
        this.salutations = loaded.collection<Salutation>(m.Salutation);
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
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }
}
