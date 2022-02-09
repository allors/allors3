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
  RequestForQuote,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../../services/fetcher/fetcher-service';
import { InternalOrganisationId } from '../../../../services/state/internal-organisation-id';
import { Filters } from '../../../../filters/filters';

@Component({
  selector: 'requestforquote-edit-form',
  templateUrl: './requestforquote-edit-form.component.html',
  providers: [ContextService, OldPanelService],
})
export class RequestForQuoteEditFormComponent
  extends AllorsFormComponent<RequestForQuote>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  request: RequestForQuote;
  quote: Quote;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[] = [];
  contacts: Person[] = [];
  internalOrganisation: InternalOrganisation;

  addContactPerson = false;
  addContactMechanism = false;
  addOriginator = false;
  previousOriginator: Party;

  private subscription: Subscription;

  customersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    panel.onPull = (pulls) => {
      if (this.panel.isCollapsed) {
        const m = this.m;
        const { pullBuilder: pull } = m;
        const x = {};

        pulls.push(
          pull.RequestForQuote({
            name: requestForQuotePullName,
            objectId: this.panel.manager.id,
            include: {
              FullfillContactMechanism: {
                PostalAddress_Country: x,
              },
              RequestItems: {
                Product: x,
              },
              Originator: x,
              ContactPerson: x,
              RequestState: x,
              DerivedCurrency: x,
              CreatedBy: x,
              LastModifiedBy: x,
            },
          }),
          pull.RequestForQuote({
            name: productQuotePullName,
            objectId: this.panel.manager.id,
            select: {
              QuoteWhereRequest: x,
            },
          })
        );
      }
    };

    panel.onPulled = (loaded) => {
      if (this.panel.isCollapsed) {
        this.request = loaded.object<RequestForQuote>(requestForQuotePullName);
        this.quote = loaded.object<Quote>(productQuotePullName);
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
          this.request = undefined;

          const m = this.m;
          const { pullBuilder: pull } = m;
          const x = {};
          const id = this.panel.manager.id;

          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
            pull.RequestForQuote({
              objectId: id,
              include: {
                DerivedCurrency: x,
                Originator: x,
                ContactPerson: x,
                RequestState: x,
                FullfillContactMechanism: {
                  PostalAddress_Country: x,
                },
              },
            }),
          ];

          this.customersFilter = Filters.customersFilter(
            m,
            this.internalOrganisationId.value
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.request = loaded.object<RequestForQuote>(this.m.RequestForQuote);
        this.currencies = loaded.collection<Currency>(this.m.Currency);

        if (this.request.Originator) {
          this.previousOriginator = this.request.Originator;
          this.update(this.request.Originator);
        }
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
    }, this.errorService.errorHandler);
  }

  get originatorIsPerson(): boolean {
    return (
      !this.request.Originator ||
      this.request.Originator.strategy.cls === this.m.Person
    );
  }

  public originatorSelected(party: IObject) {
    if (party) {
      this.update(party as Party);
    }
  }

  public partyContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.request.Originator.addPartyContactMechanism(partyContactMechanism);
    this.request.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public personAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.request
      .Originator as Organisation;
    organisationContactRelationship.Contact = person;

    this.contacts.push(person);
    this.request.ContactPerson = person;
  }

  public originatorAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.request.Originator = party;
  }

  private update(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        objectId: party.id,
        select: {
          PartyContactMechanisms: x,
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
      pull.Party({
        objectId: party.id,
        select: {
          CurrentContacts: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.request.Originator !== this.previousOriginator) {
        this.request.FullfillContactMechanism = null;
        this.request.ContactPerson = null;
        this.previousOriginator = this.request.Originator;
      }

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);
    });
  }
}
