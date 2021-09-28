import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter } from 'rxjs/operators';

import { M } from '@allors/workspace/meta/default';
import { Currency, Enumeration, InternalOrganisation, Organisation, Person } from '@allors/workspace/domain/default';
import { NavigationService, PanelService, RefreshService, SaveService, SingletonId, TestScope } from '@allors/workspace/angular/base';
import { SessionService } from '@allors/workspace/angular/core';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'person-overview-detail',
  templateUrl: './person-overview-detail.component.html',
  providers: [PanelService, SessionService],
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
    @Self() public allors: SessionService,
    @Self() public panel: PanelService,
    public refreshService: RefreshService,
    public navigationService: NavigationService,
    private saveService: SaveService,
    private singletonId: SingletonId,
    private fetcher: FetcherService
  ) {
    super();

    this.m = this.allors.workspace.configuration.metaPopulation as M;

    panel.name = 'detail';
    panel.title = 'Personal Data';
    panel.icon = 'person';
    panel.expandable = true;

    // Minimized
    const pullName = `${this.panel.name}_${this.m.Person.tag}`;

    panel.onPull = (pulls) => {
      this.person = undefined;

      if (this.panel.isCollapsed) {
        const { pullBuilder: pull } = this.m;
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
        this.person = loaded.objects[pullName] as Person;
      }
    };
  }

  public ngOnInit(): void {
    // Maximized
    this.subscription = this.panel.manager.on$
      .pipe(
        filter(() => {
          return this.panel.isExpanded;
        }),
        switchMap(() => {
          this.person = undefined;

          const m = this.allors.workspace.configuration.metaPopulation as M;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            this.fetcher.locales,
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
              predicate: new Equals({ propertyType: m.GenderType.IsActive, value: true }),
              sorting: [{ roleType: m.GenderType.Name }],
            }),
            pull.Salutation({
              predicate: new Equals({ propertyType: m.Salutation.IsActive, value: true }),
              sorting: [{ roleType: m.Salutation.Name }],
            }),
            pull.Person({
              object: id,
              select: {
                OrganisationContactRelationshipsWhereContact: x,
              },
            }),
            pull.Person({
              object: id,
              include: {
                PreferredCurrency: x,
                Gender: x,
                Salutation: x,
                Locale: x,
                Picture: x,
              },
            }),
          ];

          return this.allors.context.load(new PullRequest({ pulls }));
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.person = loaded.objects.Person as Person;
        this.internalOrganisation = loaded.objects.InternalOrganisation as Organisation;
        this.currencies = loaded.collections.Currencies as Currency[];
        this.locales = (loaded.collections.Locales as Locale[]) || [];
        this.genders = loaded.collections.GenderTypes as Enumeration[];
        this.salutations = loaded.collections.Salutations as Enumeration[];
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.allors.context.save().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }
}
