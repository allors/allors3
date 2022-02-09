import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import {
  EditIncludeHandler,
  Node,
  CreateOrEditPullHandler,
  Pull,
  IPullResult,
  PostCreatePullHandler,
} from '@allors/system/workspace/domain';
import {
  BasePrice,
  InternalOrganisation,
  Person,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { FetcherService } from '../../../../services/fetcher/fetcher-service';

@Component({
  selector: 'person-edit-form',
  templateUrl: './person-edit-form.component.html',
  providers: [OldPanelService, ContextService],
})
export class PersonEditFormComponent
  extends AllorsFormComponent<Person>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
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
    errorService: ErrorService,
    form: NgForm,
    private singletonId: SingletonId,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

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

  public save(): void {
    this.onSave();

    super.save();
  }
}
