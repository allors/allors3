import { Component, OnInit, Self, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { switchMap, filter, map } from 'rxjs/operators';

import { M } from '@allors/default/workspace/meta';
import {
  Currency,
  Enumeration,
  GenderType,
  InternalOrganisation,
  Locale,
  Person,
  Salutation,
  ContactMechanism,
  PartyContactMechanism,
  EmailAddress,
  User,
} from '@allors/default/workspace/domain';
import {
  NavigationService,
  PanelService,
  RefreshService,
  SaveService,
  SingletonId,
} from '@allors/workspace/angular/base';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  selector: 'person-overview-detail',
  templateUrl: './person-overview-detail.component.html',
  providers: [PanelService, ContextService],
})
export class PersonOverviewDetailComponent implements OnInit, OnDestroy {
  readonly m: M;

  person: Person;
  emailAddresses: string[] = [];

  internalOrganisation: InternalOrganisation;
  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  public confirmPassword: string;

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
    this.allors.context.name = this.constructor.name;
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
        const m = this.m;
        const { pullBuilder: pull } = m;
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
              predicate: {
                kind: 'Equals',
                propertyType: m.Currency.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.Currency.Name }],
            }),
            pull.GenderType({
              predicate: {
                kind: 'Equals',
                propertyType: m.GenderType.IsActive,
                value: true,
              },
              sorting: [{ roleType: m.GenderType.Name }],
            }),
            pull.Salutation({
              predicate: {
                kind: 'Equals',
                propertyType: m.Salutation.IsActive,
                value: true,
              },
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
            pull.Person({
              objectId: id,
              select: {
                PartyContactMechanisms: {
                  include: {
                    ContactMechanism: {
                      ContactMechanismType: x,
                    },
                  },
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.person = loaded.object<Person>(m.Person);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.currencies = loaded.collection<Currency>(m.Currency);
        this.locales = loaded.collection<Locale>(m.Singleton.Locales) || [];
        this.genders = loaded.collection<GenderType>(m.GenderType);
        this.salutations = loaded.collection<Salutation>(m.Salutation);

        const partyContactMechanisms: PartyContactMechanism[] =
          loaded.collection<PartyContactMechanism>(
            m.Party.PartyContactMechanisms
          );
        this.emailAddresses =
          partyContactMechanisms
            ?.filter(
              (v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress
            )
            ?.map((v) => v.ContactMechanism)
            .map((v: EmailAddress) => v.ElectronicAddressString) ?? [];
      });
  }

  private onSave(): void {
    if (
      this.person.UserEmail != null &&
      this.emailAddresses.indexOf(this.person.UserEmail) == -1
    ) {
      const emailAddress = this.allors.context.create<EmailAddress>(
        this.m.EmailAddress
      );
      emailAddress.ElectronicAddressString = this.person.UserEmail;

      const partyContactMechanism =
        this.allors.context.create<PartyContactMechanism>(
          this.m.PartyContactMechanism
        );
      partyContactMechanism.ContactMechanism = emailAddress;

      this.person.addPartyContactMechanism(partyContactMechanism);
      this.emailAddresses.push(this.person.UserEmail);
    }
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  public save(): void {
    this.onSave();

    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
      this.panel.toggle();
    }, this.saveService.errorHandler);
  }
}
