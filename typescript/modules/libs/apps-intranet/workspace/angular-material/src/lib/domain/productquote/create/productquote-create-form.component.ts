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
  ProductQuote,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '../../../services/state/internal-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';
import { Filters } from '../../../filters/filters';

@Component({
  templateUrl: './productquote-create-form.component.html',
  providers: [ContextService],
})
export class ProductQuoteCreateFormComponent
  extends AllorsFormComponent<ProductQuote>
  implements CreateOrEditPullHandler, EditIncludeHandler, PostCreatePullHandler
{
  readonly m: M;

  title = 'Add Quote';

  quote: ProductQuote;
  request: RequestForQuote;
  currencies: Currency[];
  contactMechanisms: ContactMechanism[] = [];
  contacts: Person[] = [];
  irpfRegimes: IrpfRegime[];
  vatRegimes: VatRegime[];
  showIrpf: boolean;

  addContactPerson = false;
  addContactMechanism = false;
  addReceiver = false;
  internalOrganisation: InternalOrganisation;

  private subscription: Subscription;
  private previousReceiver: Party;

  customersFilter: SearchFactory;
  currencyInitialRole: Currency;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(([, internalOrganisationId]) => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Currency({ sorting: [{ roleType: m.Currency.Name }] }),
            pull.IrpfRegime({ sorting: [{ roleType: m.IrpfRegime.Name }] }),
          ];

          this.customersFilter = Filters.customersFilter(
            m,
            internalOrganisationId
          );

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
        this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
        this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
        this.irpfRegimes = loaded.collection<IrpfRegime>(m.IrpfRegime);
        this.currencies = loaded.collection<Currency>(m.Currency);

        this.quote = this.allors.context.create<ProductQuote>(m.ProductQuote);
        this.quote.Issuer = this.internalOrganisation;
        this.quote.IssueDate = new Date();
        this.quote.ValidFromDate = new Date();
      });
  }

  get receiverIsPerson(): boolean {
    return (
      !this.quote.Receiver || this.quote.Receiver.strategy.cls === this.m.Person
    );
  }

  public receiverSelected(party: IObject): void {
    if (party) {
      this.update(party as Party);
    }
  }

  public receiverAdded(party: Party): void {
    const customerRelationship =
      this.allors.context.create<CustomerRelationship>(
        this.m.CustomerRelationship
      );
    customerRelationship.Customer = party;
    customerRelationship.InternalOrganisation = this.internalOrganisation;

    this.quote.Receiver = party;
  }

  public personAdded(person: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.quote
      .Receiver as Organisation;
    organisationContactRelationship.Contact = person;

    this.contacts.push(person);
    this.quote.ContactPerson = person;
  }

  public partyContactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    this.quote.Receiver.addPartyContactMechanism(partyContactMechanism);
    this.quote.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  private update(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: party,
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
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Party({
        object: party,
        name: 'selectedParty',
        include: {
          PreferredCurrency: x,
          Locale: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (
        this.previousReceiver &&
        this.quote.Receiver !== this.previousReceiver
      ) {
        this.quote.ContactPerson = null;
        this.quote.FullfillContactMechanism = null;
      }

      this.previousReceiver = this.quote.Receiver;

      const partyContactMechanisms: PartyContactMechanism[] =
        loaded.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );
      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );
      this.contacts = loaded.collection<Person>(m.Party.CurrentContacts);

      const selectedParty = loaded.object<Party>('selectedParty');
      this.currencyInitialRole =
        selectedParty.PreferredCurrency ?? this.quote.Issuer.PreferredCurrency;
    });
  }
}
